open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

let (+/) a b = Path.Combine(a, b)

let pkgFolder = __SOURCE_DIRECTORY__ +/ "WebSharper/Packages"
if not <| Directory.Exists pkgFolder then
    Directory.CreateDirectory pkgFolder |> ignore

Directory.EnumerateFiles(__SOURCE_DIRECTORY__ +/ "packages", "*.nupkg", SearchOption.AllDirectories)
|> Seq.iter (fun p ->
    File.Copy(p, pkgFolder +/ Path.GetFileName(p), true) 
)

let packageVersions =
    let packageRegex = Regex "([^ ]+) \(([^\)>=]+)\)"
    File.ReadAllLines("paket.lock") |> Seq.choose (fun l ->
        let c = packageRegex.Match l
        if c.Success then
            let p = c.Groups.[1].Value
            let v = c.Groups.[2].Value
            if p = "System.ValueTuple" then
                Some (p, "4.3.0")
            else
                Some (p, v)
        else
            None
    ) |> List.ofSeq

let replaces =
    [   
        for p, v in packageVersions do
            yield 
                sprintf "package id=\"%s\"" p, 
                sprintf "package id=\"%s\" version=\"%s\"" p v
            yield
                sprintf "Include=\"Packages\\%s.nupkg\"" p, 
                sprintf "Include=\"Packages\\%s.%s.nupkg\"" p v
    ]

Directory.EnumerateFiles(__SOURCE_DIRECTORY__, "*.in", SearchOption.AllDirectories)
|> Seq.iter (fun p ->
    let inp = File.ReadAllText(p)
    let res =
        (inp, replaces) ||> List.fold (fun s (i, o) -> s.Replace(i, o)) 
    File.WriteAllText(p.[.. p.Length - 4], res)
)
