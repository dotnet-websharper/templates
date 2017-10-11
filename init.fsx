open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

let (+/) a b = Path.Combine(a, b)

let pkgFolder = __SOURCE_DIRECTORY__ +/ "WebSharper.Vsix/Packages"
if not <| Directory.Exists pkgFolder then
    Directory.CreateDirectory pkgFolder |> ignore

let nupkgFiles =
    Directory.EnumerateFiles(__SOURCE_DIRECTORY__ +/ "packages", "*.nupkg", SearchOption.AllDirectories)
    |> Array.ofSeq

nupkgFiles
|> Seq.iter (fun p ->
    File.Copy(p, pkgFolder +/ Path.GetFileName(p), true) 
) 

let packageVersions =
    let nupkgFileNames =
        nupkgFiles |> Seq.map (fun p -> Path.GetFileNameWithoutExtension(p).ToLower()) |> Set.ofSeq

    let packageRegex = Regex "([^ ]+) \(([^\)>=]+)\)"
    File.ReadAllLines("paket.lock") |> Seq.choose (fun l ->
        let c = packageRegex.Match l
        if c.Success then
            let p = c.Groups.[1].Value
            let v = c.Groups.[2].Value
            // hack to include truncated .0
            let checkExists (p: string) v =
                nupkgFileNames.Contains(p.ToLower() + "." + v)
            if checkExists p v then 
                Some (p, v)
            else
                let v0 = v + ".0"
                if checkExists p v0 then 
                    Some (p, v0)
                else
                    failwithf "Could not find nupkg file for %s.%s" p v
        else
            None
    ) |> List.ofSeq |> List.distinctBy fst

printfn "Using packages:"
for p, v in packageVersions do
    printfn "  %s.%s" p v

let snk, publicKeyToken =
    match Environment.GetEnvironmentVariable "INTELLIFACTORY" with
    | null -> "../tools/WebSharper.snk", "451ee5fa653b377d"
    | p -> p +/ "keys" +/ "IntelliFactory.snk", "dcd983dec8f76a71"

let revision =
    match Environment.GetEnvironmentVariable("BUILD_NUMBER") with
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

let replacesInFile replaces p =
    let inp = File.ReadAllText(p)
    let res = (inp, replaces) ||> List.fold (fun s (i: string, o) -> s.Replace(i, o)) 
    File.WriteAllText(p.[.. p.Length - 4], res)

let vsixAssembly =
    "WebSharper." + version + tag + ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=" + publicKeyToken

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

__SOURCE_DIRECTORY__ +/ "WebSharper.Vsix/WebSharper.Vsix.csproj.in" |> replacesInFile [   
        for p, v in packageVersions do
            yield
                sprintf "Include=\"Packages\\%s.nupkg\"" p, 
                sprintf "Include=\"Packages\\%s.%s.nupkg\"" p v
        yield "{vsixversion}", version + tag
        yield "{keyfilepath}", snk
    ]

__SOURCE_DIRECTORY__ +/ "WebSharper.Vsix/source.extension.vsixmanifest.in" |> replacesInFile [   
    "{vsixversion}", version
]
