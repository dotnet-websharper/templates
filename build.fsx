#load "tools/includes.fsx"
open IntelliFactory.Build
open System
open System.IO

let bt =
    BuildTool().PackageId("Zafir.Templates")
        .VersionFrom("Zafir")
        .WithFSharpVersion(FSharpVersion.FSharp31)
        .WithFramework(fun fw -> fw.Net40)

let skiptests = fsi.CommandLineArgs |> Array.contains "skiptests"

let templates =
    bt.FSharp.Library("WebSharper.Templates")
        .SourcesFromProject()
        .References(fun r ->
            [
                r.Assembly("System.Xml")
                r.Assembly("System.Xml.Linq")
                r.NuGet("SharpCompress").Version("[0.11.6]").ForceFoundVersion().Reference()
            ])
        .References(fun r ->
            if skiptests then [] else
            [
                r.NuGet("FSharp.Core").BuildTimeOnly().Version("[4.0.0.1]").Reference()
                r.NuGet("IntelliFactory.Xml").BuildTimeOnly().Reference()
                r.NuGet("Owin").BuildTimeOnly().Reference()
                r.NuGet("Paket").BuildTimeOnly().Version("[3.35.3]").Reference()
                r.NuGet("Microsoft.Owin").BuildTimeOnly().Reference()
                r.NuGet("Microsoft.Owin.Diagnostics").BuildTimeOnly().Reference()
                r.NuGet("Microsoft.Owin.FileSystems").BuildTimeOnly().Reference()
                r.NuGet("Microsoft.Owin.Host.HttpListener").BuildTimeOnly().Reference()
                r.NuGet("Microsoft.Owin.Hosting").BuildTimeOnly().Reference()
                r.NuGet("Microsoft.Owin.StaticFiles").BuildTimeOnly().Reference()
                r.NuGet("Mono.Cecil").BuildTimeOnly().Reference()
                r.NuGet("Suave").BuildTimeOnly().Reference()
                r.NuGet("Zafir").Latest(true).BuildTimeOnly().Reference()
                r.NuGet("Zafir.CSharp").Latest(true).BuildTimeOnly().Reference()
                r.NuGet("Zafir.FSharp").Latest(true).BuildTimeOnly().Reference()
                r.NuGet("Zafir.Html").Latest(true).BuildTimeOnly().Reference()
                r.NuGet("Zafir.Owin").Latest(true).BuildTimeOnly().Reference()
                r.NuGet("Zafir.Suave").Latest(true).BuildTimeOnly().Reference()
                r.NuGet("Zafir.UI.Next").Latest(true).BuildTimeOnly().Reference()
            ])

bt.Solution [
    templates
]
|> bt.Dispatch

#r "System.Xml.Linq"
open System.Xml.Linq

let tests =
    [
        "bundle-uinext", "UINextApplication.fsproj"
        "bundle-uinext-csharp", "UINextApplication.csproj"
        "bundle-uinext-csharp-templ", "UINextApplication.csproj"
        "bundle-website", "SinglePageApplication.fsproj"
        "bundle-website-csharp", "SinglePageApplication.csproj"
        "extension", "Extension.fsproj"
        "library", "Library.fsproj"
        "library-csharp", "Library.csproj"
        "owin-selfhost", "SelfHostApplication.fsproj"
        "sitelets-host", "Web.csproj"
        "sitelets-html", "HtmlApplication.fsproj"
        "sitelets-uinext", "UI.Next.Application.fsproj"
        "sitelets-uinext-csharp", "UINextApplication.csproj"
        "sitelets-uinext-suave", "UI.Next.Application.Suave.fsproj"
        "sitelets-website", "Application.fsproj"
    ]

if skiptests then
    printfn "Skipping tests."
else
    let ( +/ ) a b = Path.Combine(a, b)

    /// recursive directory copy with overwrite and transforming files
    let rec dirCopy fromDir toDir mapFile =
        let dir = DirectoryInfo(fromDir)
        if Directory.Exists toDir |> not then
            Directory.CreateDirectory toDir |> ignore
        for f in dir.GetFiles() do
            let res = File.ReadAllText(f.FullName) |> mapFile f.FullName
            File.WriteAllText(toDir +/ f.Name, res)
        for d in dir.GetDirectories() do
            dirCopy d.FullName (toDir +/ d.Name) mapFile

    let tplDir = __SOURCE_DIRECTORY__ +/ "templates"
    let testDir = __SOURCE_DIRECTORY__ +/ "templates-test"
    if Directory.Exists(testDir) then Directory.Delete(testDir, true)
    Directory.CreateDirectory(testDir) |> ignore
    File.Copy(tplDir +/ "paket.dependencies", testDir +/ "paket.dependencies")

    let guid1 = Guid.NewGuid().ToString()
    let guid2 = Guid.NewGuid().ToString()
    tests |> List.iter (fun (dir, proj) ->
        dirCopy (tplDir +/ dir) (testDir +/ dir) <| fun fn s ->
            s.Replace("$guid1$", guid1).Replace("$guid2$", guid2)
                .Replace("$safeprojectname$", Path.GetFileNameWithoutExtension proj)
    )

    let runInTestDir exe args =
        printfn "%s %s" exe args
        let pi =
            System.Diagnostics.ProcessStartInfo(
                FileName = (__SOURCE_DIRECTORY__ +/ exe),
                Arguments = args,
                WorkingDirectory = testDir,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true)
        let p = System.Diagnostics.Process.Start(pi)
        Console.Write(p.StandardOutput.ReadToEnd())
        Console.Write(p.StandardError.ReadToEnd())
        p.WaitForExit()
        if p.ExitCode <> 0 then
            failwithf "Tests: %s exited with code %i" (Path.GetFileNameWithoutExtension exe) p.ExitCode

    let msbuild =
        [
            match System.Environment.GetEnvironmentVariable("ProgramFiles") with
            | null -> ()
            | f -> yield f +/ "MSBuild/14.0/Bin/MSBuild.exe"
            match System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") with
            | null -> ()
            | f -> yield f +/ "MSBuild/14.0/Bin/MSBuild.exe"
        ]
        |> List.find File.Exists

    if File.Exists("packages/Owin.1.0/Owin.1.0.nupkg") then
        File.Move("packages/Owin.1.0/Owin.1.0.nupkg", "packages/Owin.1.0/Owin.1.0.0.nupkg")
    runInTestDir "packages/Paket.3.35.3/tools/paket.exe" "update" // Create paket.lock with latest versions
    runInTestDir "packages/Paket.3.35.3/tools/paket.exe" "install" // Write references to .[fc]sproj files
    tests |> List.iter (fun (dir, proj) -> runInTestDir msbuild (dir +/ proj))
    printfn "Compilation tests successful."

bt.Solution [
    bt.NuGet.CreatePackage()
        .Configure(fun c ->
            { c with
                Title = Some "Zafir.Templates"
                LicenseUrl = Some "http://websharper.com/licensing"
                ProjectUrl = Some "https://github.com/intellifactory/websharper.visualstudio"
                Description = "Zafir Project Templates"
                RequiresLicenseAcceptance = true })
        .Add(templates)
    |> Array.foldBack (fun f n -> n.AddFile(f)) (
        let templatesDir = DirectoryInfo("templates").FullName
        Directory.GetFiles(templatesDir, "*", SearchOption.AllDirectories)
        |> Array.choose (fun fullPath ->
            if fullPath.Contains("paket") then None else
            Some (fullPath, "templates/" + fullPath.[templatesDir.Length + 1 ..].Replace('\\', '/')))
    )
]
|> bt.Dispatch
