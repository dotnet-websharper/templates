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
nuget FSharp.Core 5.0.0
nuget FAKE.Core
nuget Fake.Core.Target
nuget Fake.IO.FileSystem
nuget Fake.Tools.Git
nuget Fake.DotNet.Cli
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Paket
nuget Paket.Core prerelease //"
#endif

#load "paket-files/wsbuild/github.com/dotnet-websharper/build-script/WebSharper.Fake.fsx"
open WebSharper.Fake

open System.IO
open Paket.Constants
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators

let mutable taggedVersion = ""

let snk, publicKeyToken =
    match Environment.environVarOrNone "INTELLIFACTORY" with
    | None -> "../tools/WebSharper.snk", "451ee5fa653b377d"
    | Some p -> p </> "keys/IntelliFactory.snk", "dcd983dec8f76a71"

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
            n, p.Version.AsString
        )
        |> List.ofSeq

    let revision =
        match Environment.environVarOrNone "BUILD_NUMBER" with
        | None | Some "" -> "0"
        | Some r -> r

    let version, tag = 
        let wsVersion =
            packageVersions |> List.pick (function "WebSharper", v -> Some v | _ -> None)
        printfn "WebSharper version: %s" wsVersion
        let withoutTag, tag =
            match wsVersion.IndexOf('-') with
            | -1 -> wsVersion, ""
            | i -> wsVersion.[.. i - 1], wsVersion.[i ..]
        let nums = withoutTag.Split('.')
        (nums.[0 .. 2] |> String.concat ".") + "." + revision, tag

    taggedVersion <- version + tag

    printfn "WebSharper.Templates version: %s" taggedVersion

    let replacesInFile replaces p =
        let inp = File.ReadAllText(p)
        let res = (inp, replaces) ||> List.fold (fun s (i: string, o) -> s.Replace(i, o)) 
        let fn = p.[.. p.Length - 4]
        printfn "Created: %s" fn
        File.WriteAllText(fn, res)

    __SOURCE_DIRECTORY__ </> "WebSharper.Templates/WebSharper.Templates.csproj.in" |> replacesInFile [   
        "{nugetversion}", taggedVersion
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

let msbuild mode =
    MSBuild.build (fun p ->
        { p with
            Targets = [ "Restore"; "Build" ]
            Properties = ["Configuration", mode; "AssemblyOriginatorKeyFile", snk; "AssemblyName", "WebSharper." + taggedVersion]
            Verbosity = MSBuildVerbosity.Minimal |> Some
            DisableInternalBinLog = true
        }) "WebSharper.Vsix.sln"

let targets = MakeTargets { 
    WSTargets.Default (LazyVersionFrom "WebSharper") with
        BuildAction =
            BuildAction.Custom <| fun mode -> msbuild (mode.ToString())
}

Target.create "PackageTemplates" <| fun _ ->
    DotNet.pack (fun p ->
        { p with
            OutputPath = Some (Environment.environVarOrNone "WSPackageFolder" |> Option.defaultValue "build")  
            MSBuildParams = { p.MSBuildParams with
                                Verbosity = MSBuildVerbosity.Minimal |> Some
                                Properties = ["Configuration", "Release"; "AssemblyOriginatorKeyFile", snk; "AssemblyName", "WebSharper." + taggedVersion]
                                DisableInternalBinLog = true
                            }
        }) "WebSharper.Templates/WebSharper.Templates.csproj"

"WS-Update" 
    ==> "SetVersions"
    ==> "WS-Restore"

"WS-BuildRelease"
    ==> "WS-Package"

"WS-BuildRelease"
    ==> "PackageTemplates" 
    ==> "WS-Package"

Target.runOrDefault "WS-Package"
