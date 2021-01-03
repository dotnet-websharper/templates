#if INTERACTIVE
#r "nuget: FAKE.Core"
#r "nuget: Fake.Core.Target"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Fake.Tools.Git"
#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.DotNet.AssemblyInfoFile"
#r "nuget: Fake.DotNet.Paket"
#r "nuget: Paket.Core"
#else
#r "paket:
nuget FAKE.Core
nuget Fake.Core.Target
nuget Fake.IO.FileSystem
nuget Fake.Tools.Git
nuget Fake.DotNet.Cli
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Paket
nuget Paket.Core //"
#endif

open System.IO
open Paket.Constants
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators

let mutable taggedVersion = ""

Target.create "SetVersions" <| fun _ ->

    let lockFile = 
        __SOURCE_DIRECTORY__ </> "paket.lock"
        |> Paket.LockFile.LoadFrom 

    let mainGroup =
        lockFile.GetGroup(MainDependencyGroup)
    
    let packages = __SOURCE_DIRECTORY__ </> "packages"
    let nupkgPath n v = packages </> n </> (n + "." + v + ".nupkg") 

    let packageVersions = 
        mainGroup.Resolution 
        |> Map.toSeq
        |> Seq.map (fun (i, p) ->
            let n = i.Name
            let version = 
                // version in lock file might not be the full one in the file name
                let v = p.Version.AsString
                let n' = nupkgPath n v
                if File.exists n' then v else
                let v = v + ".0"
                let n = nupkgPath n v
                if File.exists n then v else
                v + ".0"
            n, version
        )
        |> List.ofSeq

    let pkgFolder = __SOURCE_DIRECTORY__ </> "WebSharper.Vsix/Packages"
    if Directory.Exists pkgFolder then
        Directory.delete pkgFolder
    Directory.create pkgFolder

    packageVersions
    |> Seq.iter (fun (n, v) ->
        let nupkgFrom = nupkgPath n v
        let nupkgTo = pkgFolder </> Path.GetFileName nupkgFrom
        File.Copy(nupkgFrom, nupkgTo)
    ) 

    let snk, publicKeyToken =
        match Environment.environVarOrNone "INTELLIFACTORY" with
        | None -> "../tools/WebSharper.snk", "451ee5fa653b377d"
        | Some p -> p </> "keys/IntelliFactory.snk", "dcd983dec8f76a71"

    let revision =
        match Environment.environVarOrNone "BUILD_NUMBER" with
        | None | Some "" -> "0"
        | Some r -> r

    let version, tag = 
        let wsVersion =
            packageVersions |> List.pick (function "WebSharper", v -> Some v | _ -> None)
        let withoutTag, tag =
            match wsVersion.IndexOf('-') with
            | -1 -> wsVersion, ""
            | i -> wsVersion.[.. i - 1], wsVersion.[i ..]
        let nums = withoutTag.Split('.')
        (nums.[0 .. 2] |> String.concat ".") + "." + revision, tag

    taggedVersion <- version + tag

    let replacesInFile replaces p =
        let inp = File.ReadAllText(p)
        let res = (inp, replaces) ||> List.fold (fun s (i: string, o) -> s.Replace(i, o)) 
        let fn = p.[.. p.Length - 4]
        printfn "Created: %s" fn
        File.WriteAllText(fn, res)

    let vsixAssembly =
        "WebSharper." + taggedVersion + ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=" + publicKeyToken

    let vstemplateReplaces =
        [   
            for p, v in packageVersions do
                yield 
                    sprintf "package id=\"%s\"" p, 
                    sprintf "package id=\"%s\" version=\"%s\"" p v
            yield "{vsixassembly}", vsixAssembly
        ]

    Directory.EnumerateFiles(__SOURCE_DIRECTORY__, "*.vstemplate.in", SearchOption.AllDirectories)
    |> Seq.iter (replacesInFile vstemplateReplaces)

    __SOURCE_DIRECTORY__ </> "WebSharper.Vsix/WebSharper.Vsix.csproj.in" |> replacesInFile [   
            for p, v in packageVersions do
                yield
                    sprintf "Include=\"Packages\\%s.nupkg\"" p, 
                    sprintf "Include=\"Packages\\%s.%s.nupkg\"" p v
            yield "{vsixversion}", taggedVersion
            yield "{keyfilepath}", snk
        ]

    __SOURCE_DIRECTORY__ </> "WebSharper.Vsix/source.extension.vsixmanifest.in" |> replacesInFile [   
        "{vsixversion}", version
    ]

    let dotnetProjReplaces =
        [   
            for p, v in packageVersions do
                yield 
                    sprintf "Include=\"%s\"" p, 
                    sprintf "Include=\"%s\" Version=\"%s\"" p v
        ]

    Directory.EnumerateFiles(__SOURCE_DIRECTORY__, "*.FSharp.fsproj.in", SearchOption.AllDirectories)
    |> Seq.iter (replacesInFile dotnetProjReplaces)

    Directory.EnumerateFiles(__SOURCE_DIRECTORY__, "*.CSharp.csproj.in", SearchOption.AllDirectories)
    |> Seq.iter (replacesInFile dotnetProjReplaces)

    let wsRef = """    <PackageReference Include="WebSharper" """

    let ancNugetRef =
        """    $if$ ($visualstudioversion$ < 16.0)<PackageReference Include="Microsoft.AspNetCore.All" Version="2.0.8" />
        $endif$<PackageReference Include="WebSharper" """

    Directory.EnumerateDirectories(__SOURCE_DIRECTORY__ </> "NetCore")
    |> Seq.iter (fun ncPath ->
        match Path.GetFileName(ncPath).Split('-') with
        | [| name; lang |] ->
            let vcPath = __SOURCE_DIRECTORY__ </> lang </> (name + "-NetCore")
            Directory.EnumerateFiles(ncPath, "*.*", SearchOption.AllDirectories)
            |> Seq.iter (fun f ->
                if not (f.Contains("\\bin") || f.Contains("\\obj") || f.Contains("\\.template.config") || f.EndsWith(".in") || f.EndsWith(".user")) then
                    let fn =
                        if f.EndsWith("proj") then 
                            "ProjectTemplate." + (if lang = "CSharp" then "csproj" else "fsproj")    
                        else f
                    let copyTo =
                        vcPath </> Fake.IO.Path.toRelativeFrom ncPath fn
                    printfn "Copied: %s -> %s" f copyTo
                    Directory.CreateDirectory(Path.GetDirectoryName(copyTo)) |> ignore
                    let res = 
                        File.ReadAllText(f)
                            .Replace(sprintf "WebSharper.%s.%s" name lang, "$safeprojectname$")
                            .Replace("IWebHostEnvironment", "$if$ ($visualstudioversion$ >= 16.0)IWebHostEnvironment$else$IHostingEnvironment$endif$")
                    let res =
                        if res.Contains("netcoreapp3.1") then
                            res
                                .Replace("netcoreapp3.1", "$aspnetcoreversion$")
                                .Replace(wsRef, ancNugetRef)
                        else
                            res
                
                    File.WriteAllText(copyTo, res)
            )
        | _ -> ()
)

//Shell.Exec(
//    "tools/nuget/NuGet.exe",
//    sprintf "pack -Version %s -OutputDirectory build WebSharper.Templates.nuspec" version
//)

//match Environment.environVarOrNone "NugetPublishUrl", Environment.environVarOrNone "NugetApiKey" with
//| Some nugetPublishUrl, Some nugetApiKey ->
//    Trace.logfn "[NUGET] Publishing to %s" nugetPublishUrl 
//    Paket.push <| fun p ->
//        { p with
//            PublishUrl = nugetPublishUrl
//            ApiKey = nugetApiKey
//            WorkingDir = "build"
//        }
//| _ -> Trace.traceError "[NUGET] Not publishing: NugetPublishUrl and/or NugetApiKey are not set"

Target.create "Package" <| fun _ ->
    Paket.pack <| fun p ->
        { p with
            ToolType = ToolType.CreateLocalTool()
            OutputPath = "build"
            Version = taggedVersion
        }

Target.create "Push" <| fun _ ->
    match Environment.environVarOrNone "NugetPublishUrl", Environment.environVarOrNone "NugetApiKey" with
    | Some nugetPublishUrl, Some nugetApiKey ->
        Trace.logfn "[NUGET] Publishing to %s" nugetPublishUrl 
        Paket.push <| fun p ->
            { p with
                PublishUrl = nugetPublishUrl
                ApiKey = nugetApiKey
                WorkingDir = "build"
            }
    | _ -> Trace.traceError "[NUGET] Not publishing: NugetPublishUrl and/or NugetApiKey are not set"


let msbuild o mode =
    MSBuild.build (fun p ->
        { p with
            Properties = ["Configuration", mode]
        }) "WebSharper.Vsix.sln"

Target.create "BuildDebug" <| fun o ->
    msbuild o "Debug"

Target.create "BuildRelease" <| fun o ->
    msbuild o "Release"

Target.create "CI-Release" ignore

"SetVersions" 
    ==> "BuildDebug"

"SetVersions" 
    ==> "BuildRelease"
    ==> "Package"
    ==> "Push"
    ==> "CI-Release"

Target.runOrDefault "BuildRelease"
