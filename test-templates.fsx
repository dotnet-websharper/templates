// Tests a WebSharper.Templates package end to end: instantiates every template with
// `dotnet new`, builds them in one solution in Release, then starts the web projects
// and checks that they serve their pages and the scripts those pages reference.
//
// Usage: dotnet fsi test-templates.fsx <WebSharper.Templates nupkg> <work folder>
//
// The work folder is deleted and recreated. The package is installed into a custom
// template hive inside it, so templates installed for the user are left untouched.
// The folder of the package is added as a NuGet source, so WebSharper packages built
// locally next to it (WSPackageFolder) are found too.

open System
open System.Collections.Concurrent
open System.Diagnostics
open System.IO
open System.IO.Compression
open System.Net
open System.Net.Http
open System.Net.Sockets
open System.Text.RegularExpressions
open System.Threading

let nupkg, workDir =
    match fsi.CommandLineArgs |> Array.skip 1 with
    | [| nupkg; workDir |] -> Path.GetFullPath nupkg, Path.GetFullPath workDir
    | _ -> failwith "Usage: dotnet fsi test-templates.fsx <WebSharper.Templates nupkg> <work folder>"

if not (File.Exists nupkg) then
    failwithf "Template package not found: %s" nupkg

// Child processes use the same dotnet that runs this script.
let dotnet = Process.GetCurrentProcess().MainModule.FileName
let hive = Path.Combine(workDir, ".hive")

type Check =
    /// Building is the whole test (libraries, extensions, proxies).
    | BuildOnly
    /// Run the project, GET each path and check that the response contains the given text.
    | Site of (string * string) list
    /// Check the static HTML output folder and the local scripts its pages reference.
    | StaticHtml of outputDir: string

type TemplateTest =
    {
        ShortName: string
        Lang: string
        Name: string
        Check: Check
    }

let tests =
    [
        { ShortName = "websharper-lib"; Lang = "F#"; Name = "Fs.Lib"; Check = BuildOnly }
        { ShortName = "websharper-lib"; Lang = "C#"; Name = "Cs.Lib"; Check = BuildOnly }
        { ShortName = "websharper-ext"; Lang = "F#"; Name = "Fs.Ext"; Check = BuildOnly }
        { ShortName = "websharper-prx"; Lang = "F#"; Name = "Fs.Prx"; Check = BuildOnly }
        { ShortName = "websharper-prx"; Lang = "C#"; Name = "Cs.Prx"; Check = BuildOnly }
        { ShortName = "websharper-web"; Lang = "F#"; Name = "Fs.Web"; Check = Site [ "/", "<html"; "/about", "<html" ] }
        { ShortName = "websharper-web"; Lang = "C#"; Name = "Cs.Web"; Check = Site [ "/", "<html"; "/about", "<html" ] }
        { ShortName = "websharper-spa"; Lang = "F#"; Name = "Fs.Spa"; Check = Site [ "/", "<html" ] }
        { ShortName = "websharper-spa"; Lang = "C#"; Name = "Cs.Spa"; Check = Site [ "/", "<html" ] }
        { ShortName = "websharper-min"; Lang = "F#"; Name = "Fs.Min"; Check = Site [ "/", "Hello World" ] }
        { ShortName = "websharper-min"; Lang = "C#"; Name = "Cs.Min"; Check = Site [ "/", "Hello World" ] }
        { ShortName = "websharper-svc"; Lang = "F#"; Name = "Fs.Svc"; Check = Site [ "/", "Service version 1.0"; "/user/42", "John" ] }
        { ShortName = "websharper-svc"; Lang = "C#"; Name = "Cs.Svc"; Check = Site [ "/", "Service version 1.0"; "/user/42", "John" ] }
        { ShortName = "websharper-html"; Lang = "F#"; Name = "Fs.Html"; Check = StaticHtml "build" }
        { ShortName = "websharper-html"; Lang = "C#"; Name = "Cs.Html"; Check = StaticHtml "build" }
    ]

/// Project references added on top of the templates, to also test cross-project compilation.
let references =
    [
        "Fs.Web", [ "Fs.Lib"; "Fs.Ext"; "Cs.Lib" ]
    ]

let projectFile (name: string) =
    let dir = Path.Combine(workDir, name)
    Directory.GetFiles(dir, name + ".*proj") |> Array.exactlyOne

let run (dir: string) (arguments: string list) =
    printfn "> dotnet %s" (String.Join(" ", arguments))
    let psi = ProcessStartInfo(dotnet, WorkingDirectory = dir, UseShellExecute = false)
    for a in arguments do psi.ArgumentList.Add a
    use proc = Process.Start psi
    proc.WaitForExit()
    if proc.ExitCode <> 0 then
        failwithf "'dotnet %s' failed in %s (exit code %d)" (String.Join(" ", arguments)) dir proc.ExitCode

let http = new HttpClient(Timeout = TimeSpan.FromSeconds 30.)

let tryGet (url: string) =
    try
        use resp = http.GetAsync(url).Result
        Ok (int resp.StatusCode, resp.Content.ReadAsStringAsync().Result)
    with e ->
        Error e.InnerException

let get (url: string) =
    match tryGet url with
    | Ok (status, body) when status >= 200 && status < 300 -> body
    | Ok (status, _) -> failwithf "GET %s returned status %d" url status
    | Error e -> failwithf "GET %s failed: %s" url e.Message

let scriptRegex = Regex("""<script[^>]*\ssrc\s*=\s*["']([^"']+)["']""", RegexOptions.IgnoreCase)
let styleRegex = Regex("""<link[^>]*\shref\s*=\s*["']([^"']+\.css)(?:[?#][^"']*)?["']""", RegexOptions.IgnoreCase)

let localUrls (regex: Regex) (html: string) =
    [ for m in regex.Matches html do
        let src = m.Groups[1].Value
        if not (Regex.IsMatch(src, "^([a-z]+:|//|#)", RegexOptions.IgnoreCase)) then
            yield src.Split('?', '#')[0] ]
    |> List.distinct

/// Local scripts referenced by a page, these must exist.
let localScripts html = localUrls scriptRegex html

/// Local stylesheets referenced by a page. Pages link the bundle's stylesheet even when the
/// bundle has no CSS, so a missing one is only reported.
let localStyles html = localUrls styleRegex html

let freePort () =
    let listener = new TcpListener(IPAddress.Loopback, 0)
    listener.Start()
    let port = (listener.LocalEndpoint :?> IPEndPoint).Port
    listener.Stop()
    port

let testSite (t: TemplateTest) (checks: (string * string) list) =
    let projDir = Path.Combine(workDir, t.Name)
    let dll =
        Directory.GetFiles(Path.Combine(projDir, "bin", "Release"), t.Name + ".dll", SearchOption.AllDirectories)
        |> Array.head
    let baseUrl = sprintf "http://127.0.0.1:%d" (freePort ())
    let psi =
        ProcessStartInfo(dotnet, WorkingDirectory = projDir, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true)
    for a in [ dll; "--urls"; baseUrl ] do psi.ArgumentList.Add a
    let output = ConcurrentQueue<string>()
    use proc = new Process(StartInfo = psi)
    proc.OutputDataReceived.Add(fun e -> if not (isNull e.Data) then output.Enqueue e.Data)
    proc.ErrorDataReceived.Add(fun e -> if not (isNull e.Data) then output.Enqueue e.Data)
    proc.Start() |> ignore
    proc.BeginOutputReadLine()
    proc.BeginErrorReadLine()
    let fail msg =
        failwithf "%s: %s\nServer output:\n%s" t.Name msg (String.Join("\n", output))
    try
        // wait until the server answers
        let deadline = DateTime.Now.AddSeconds 60.
        let rec waitForStart () =
            if proc.HasExited then fail (sprintf "server exited with code %d" proc.ExitCode)
            match tryGet (baseUrl + "/") with
            | Ok _ -> ()
            | Error _ when DateTime.Now < deadline -> Thread.Sleep 500; waitForStart ()
            | Error e -> fail ("server did not start: " + e.Message)
        waitForStart ()
        for path, expected in checks do
            let body = try get (baseUrl + path) with e -> fail e.Message
            if not (body.Contains expected) then
                fail (sprintf "GET %s does not contain '%s'" path expected)
            let assetUrl (asset: string) =
                if asset.StartsWith "/" then baseUrl + asset
                else Uri(Uri(baseUrl + path), asset).ToString()
            for script in localScripts body do
                let url = assetUrl script
                try get url |> ignore with e -> fail e.Message
                printfn "  %s%s -> %s ok" t.Name path url
            for style in localStyles body do
                match tryGet (assetUrl style) with
                | Ok (status, _) when status >= 200 && status < 300 -> ()
                | Ok (status, _) -> printfn "  warning: %s%s -> %s returned status %d" t.Name path style status
                | Error e -> printfn "  warning: %s%s -> %s failed: %s" t.Name path style e.Message
        printfn "%s: site ok" t.Name
    finally
        if not proc.HasExited then
            proc.Kill true
            proc.WaitForExit()

let testStaticHtml (t: TemplateTest) (outputDir: string) =
    let dir = Path.Combine(workDir, t.Name, outputDir)
    let pages =
        if Directory.Exists dir then Directory.GetFiles(dir, "*.html", SearchOption.AllDirectories) else [||]
    if Array.isEmpty pages then
        failwithf "%s: no HTML pages generated in %s" t.Name dir
    for page in pages do
        let html = File.ReadAllText page
        let assetPath (asset: string) =
            if asset.StartsWith "/" then Path.Combine(dir, asset.TrimStart '/')
            else Path.Combine(Path.GetDirectoryName page, asset)
        for script in localScripts html do
            if not (File.Exists (assetPath script)) then
                failwithf "%s: %s references missing script %s" t.Name page script
        for style in localStyles html do
            if not (File.Exists (assetPath style)) then
                printfn "  warning: %s: %s references missing stylesheet %s" t.Name page style
    printfn "%s: %d static pages ok" t.Name pages.Length

/// The F# booster (wsfscservice) keeps running after a build, with its working directory in
/// the first project it compiled, which keeps the work folder locked on Windows. Stop the
/// boosters of the WebSharper.FSharp versions referenced by the templates.
let boosterVersions =
    use zip = ZipFile.OpenRead nupkg
    zip.Entries
    |> Seq.filter (fun e -> e.FullName.EndsWith ".fsproj")
    |> Seq.collect (fun e ->
        use reader = new StreamReader(e.Open())
        Regex.Matches(reader.ReadToEnd(), """Include="WebSharper\.FSharp"\s+Version="([^"]+)" """.TrimEnd())
        |> Seq.map (fun m -> m.Groups[1].Value.ToLowerInvariant())
        |> List.ofSeq)
    |> Seq.distinct
    |> List.ofSeq

let stopBoosters () =
    let sep = string Path.DirectorySeparatorChar
    for proc in Process.GetProcessesByName "wsfscservice" do
        let path = try proc.MainModule.FileName.ToLowerInvariant() with _ -> ""
        if boosterVersions |> List.exists (fun v -> path.Contains("websharper.fsharp" + sep + v + sep)) then
            printfn "Stopping wsfscservice (PID %d)" proc.Id
            proc.Kill()
            proc.WaitForExit()

stopBoosters ()
if Directory.Exists workDir then
    Directory.Delete(workDir, true)
Directory.CreateDirectory workDir |> ignore

let nugetConfig =
    String.concat "\n" [
        """<?xml version="1.0" encoding="utf-8"?>"""
        "<configuration>"
        "  <packageSources>"
        sprintf """    <add key="websharper-local-build" value="%s" />""" (Path.GetDirectoryName nupkg)
        "  </packageSources>"
        "</configuration>"
    ]
File.WriteAllText(Path.Combine(workDir, "NuGet.Config"), nugetConfig)

try
    run workDir [ "new"; "install"; nupkg; "--debug:custom-hive"; hive ]
    run workDir [ "new"; "sln"; "-n"; "TemplateTests" ]
    let solution = Directory.GetFiles(workDir, "TemplateTests.sln*") |> Array.exactlyOne

    for t in tests do
        run workDir [ "new"; t.ShortName; "-lang"; t.Lang; "-o"; t.Name; "--debug:custom-hive"; hive ]
        run workDir [ "sln"; solution; "add"; projectFile t.Name ]

    for project, refs in references do
        run workDir ([ "add"; projectFile project; "reference" ] @ List.map projectFile refs)

    run workDir [ "build"; solution; "-c"; "Release"; "-nr:false" ]

    for t in tests do
        match t.Check with
        | BuildOnly -> printfn "%s: build ok" t.Name
        | Site checks -> testSite t checks
        | StaticHtml outputDir -> testStaticHtml t outputDir
finally
    stopBoosters ()

printfn "All %d templates passed." tests.Length
