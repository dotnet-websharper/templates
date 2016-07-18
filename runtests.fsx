#load "tools/includes.fsx"
open IntelliFactory.Build
open System.IO
open System

let ( +/ ) a b = Path.Combine(a, b)

/// recursive directory copy with overwrite and transforming files
let rec dirCopy fromDir toDir mapFile =
    let dir = DirectoryInfo(fromDir)
    if Directory.Exists toDir |> not then
        Directory.CreateDirectory toDir |> ignore
    for f in dir.GetFiles() do
        let res = File.ReadAllText(f.FullName) |> mapFile
        File.WriteAllText(toDir +/ f.Name, res)
    for d in dir.GetDirectories() do
        dirCopy d.FullName (toDir +/ d.Name) mapFile

let testDir = __SOURCE_DIRECTORY__ +/ "templates-test"

dirCopy (__SOURCE_DIRECTORY__ +/ "templates") testDir <| fun s ->
    s.Replace("$guid1$", Guid.NewGuid().ToString()).Replace("$safeprojectname$", "TemplateTest")

// TODO: build the templates