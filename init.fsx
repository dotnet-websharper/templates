#r "packages/build/FAKE/tools/FakeLib.dll"
#r "packages/build/Chessie/lib/net40/Chessie.dll"
#r "packages/build/Paket.Core/lib/net45/Paket.Core.dll"

open Fake
open System.IO

let parsedLock = 
    __SOURCE_DIRECTORY__ </> "paket.lock"
    |> File.ReadAllLines 
    |> Paket.LockFileParser.Parse |> List.last
    
let packageVersions = 
    parsedLock.Packages |> List.map (fun p ->
        let packageName = p.Name.Name
        packageName, p.Version.AsString
    )

let pkgFolder = __SOURCE_DIRECTORY__ </> "WebSharper.Vsix/Packages"
CreateDir pkgFolder

let nupkgFiles = !! "packages/**/*.nupkg"

nupkgFiles
|> Seq.iter (fun p ->
    File.Copy(p, pkgFolder </> Path.GetFileName(p), true) 
) 

let snk, publicKeyToken =
    match environVar "INTELLIFACTORY" with
    | null -> "../tools/WebSharper.snk", "451ee5fa653b377d"
    | p -> p </> "keys/IntelliFactory.snk", "dcd983dec8f76a71"

let revision =
    match environVar "BUILD_NUMBER" with
    | null | "" -> "0"
    | r -> r

let version, tag = 
    let wsVersion =
        packageVersions |> List.pick (function "WebSharper", v -> Some v | _ -> None)
    let withoutTag, tag =
        match wsVersion.IndexOf('-') with
        | -1 -> wsVersion, ""
        | i -> wsVersion.[.. i - 1], wsVersion.[i ..]
    let nums = withoutTag.Split('.')
    (nums.[0 .. 2] |> String.concat ".") + "." + revision, tag

let taggedVersion = version + tag

let replacesInFile replaces p =
    let inp = File.ReadAllText(p)
    let res = (inp, replaces) ||> List.fold (fun s (i: string, o) -> s.Replace(i, o)) 
    File.WriteAllText(p.[.. p.Length - 4], res)

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

!! "NetCore/**/wwwroot/**" 
++ "NetCore/**/bin/**" 
++ "NetCore/**/obj/**" 
++ "NetCore/**/*.user" 
++ "NetCore/**/Properties/launchSettings.json"
|> DeleteFiles

Shell.Exec(
    "tools/nuget/NuGet.exe",
    sprintf "pack -Version %s -OutputDirectory build WebSharper.Templates.nuspec" taggedVersion
)
