// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2014 IntelliFactory
//
// GNU Affero General Public License Usage
// WebSharper is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License, version 3, as published
// by the Free Software Foundation.
//
// WebSharper is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License
// for more details at <http://www.gnu.org/licenses/>.
//
// If you are unsure which license is appropriate for your use, please contact
// IntelliFactory at http://intellifactory.com/contact.
//
// $end{copyright}

namespace WebSharper.Templates

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Xml
open System.Xml.Linq

type LocalSource =
    {
        FileSet : FileSet
        TargetsFile : string
        LibDir : string
    }

type NuGetPackage =
    | PkgBytes of byte[]
    | PkgLatestPublic

type NuGetSource =
    {
        WebSharperNuGetPackage : option<NuGetPackage>
        WebSharperTemplatesNuGetPackage : NuGetPackage
        PackagesDirectory : string
    }

type Source =
    | SLocal of LocalSource
    | SNuGet of NuGetSource

type InitOptions =
    {
        Directory : string
        ProjectName : string
        Source : Source
    }

type Template =
    | T of string

[<AutoOpen>]
module Implementation =

    let CheckNuGetOpts opts =
        if IsFile opts.PackagesDirectory then
            [sprintf "Invalid PackagesDirectory: %s" opts.PackagesDirectory]
        else []

    let CheckSource src =
        match src with
        | SLocal loc ->
            [
                if NotFile loc.TargetsFile then
                    yield sprintf "WebSharper.targets not found at %s" loc.TargetsFile
            ]
        | _ -> []

    let Complain fmt =
        Printf.ksprintf (fun str -> "TemplateOptions: " + str) fmt

    let Check opts =
        [
            yield! CheckSource opts.Source
            if IsFile opts.Directory then
                yield sprintf "Specified InitOptions.Directory is a file: %s" opts.Directory
        ]

    let UnsafeChars =
        Regex("[^_0-9a-zA-Z]")

    let Clean str =
        match str with
        | null | "" -> "MyProject"
        | str ->
            let str = UnsafeChars.Replace(str, "_")
            if Char.IsDigit(str.[0]) then
                "_" + str
            else
                str

    let Prepare opts =
        match Check opts with
        | [] ->
            EnsureDir opts.Directory
            { opts with ProjectName = Clean opts.ProjectName }
        | errors ->
            failwith (String.concat Environment.NewLine errors)

    let IsTextFile path =
        match Path.GetExtension(path) with
        | ".asax" | ".config" | ".cs" | ".csproj"
        | ".files" | ".fs" | ".fsproj" | ".fsx" | ".html" -> true
        | _ -> false

    let NeutralEncoding =
        let bom = false
        UTF8Encoding(bom, throwOnInvalidBytes = true) :> Encoding

    let CopyTextFile source target =
        let lines = File.ReadAllLines(source)
        File.WriteAllLines(target, lines, NeutralEncoding)

    let CopyFile src tgt =
        if IsTextFile src then
            CopyTextFile src tgt
        else
            File.Copy(src, tgt)

    let rec CopyDir src tgt =
        EnsureDir tgt
        for d in Directory.EnumerateDirectories(src) do
            CopyDir d (Path.Combine(tgt, Path.GetFileName(d)))
        for f in Directory.EnumerateFiles(src) do
            CopyFile f (Path.Combine(tgt, Path.GetFileName(f)))

    let Replace (a: string) (b: string) (main: string) =
        main.Replace(a, b)

    let IsProjectFile path =
        match Path.GetExtension(path) with
        | ".csproj" | ".fsproj" -> true
        | _ -> false

    let MoveProjectFile opts path =
        if IsProjectFile path then
            let ext = Path.GetExtension(path)
            let tgt = Path.Combine(Path.GetDirectoryName(path), opts.ProjectName + ext)
            File.Move(path, tgt)

    let All opts =
        Directory.EnumerateFiles(opts.Directory, "*.*", SearchOption.AllDirectories)

    let MoveProjectFiles opts =
        let projFiles = All opts |> Seq.filter IsProjectFile |> Seq.toArray
        for p in projFiles do
            MoveProjectFile opts p

    let FreshGuid () =
        Guid.NewGuid().ToString()

    let ExpandVariables opts path =
        if IsTextFile path then
            let t =
                File.ReadAllText(path)
                |> Replace "$guid1$" (FreshGuid ())
                |> Replace "$guid2$" (FreshGuid ())
                |> Replace "$safeprojectname$" opts.ProjectName
            File.WriteAllText(path, t, NeutralEncoding)

    let ExpandAllVariables opts =
        for p in All opts do
            ExpandVariables opts p

    /// Compute a path equivalent to `path`, but relative to `baseDir`.
    /// For ex. `RelPath "c:/foo/bar" "c:/foo/baz/qux.txt" = "../baz/qux.txt"`
    let RelPath baseDir path =
        let baseDir = Path.GetFullPath(baseDir)
        let path = Path.GetFullPath(path)
        let chars = [| Path.AltDirectorySeparatorChar; Path.DirectorySeparatorChar |]
        let split (path: string) =
            path.Split(chars, StringSplitOptions.RemoveEmptyEntries)
            |> Array.toList
        let rec loop baseDir path =
            match baseDir, path with
            | x :: xs, y :: ys when x = y ->
                loop xs ys
            | _ ->
                [
                    for i in 1 .. baseDir.Length do
                        yield ".."
                    yield! path
                ]
                |> String.concat "/"
        loop (split baseDir) (split path)

    let InstallRefsTo (ws: LocalSource) projectFilePath =
        let getRelPath p = RelPath (Path.GetDirectoryName(projectFilePath)) p
        let targetsRelPath = getRelPath ws.TargetsFile
        let doc = XDocument.Parse(File.ReadAllText(projectFilePath))
        let ns = doc.Root.Name.Namespace
        let imp = ns.GetName("Import")
        do // Install .targets file
            let proj = XName.Get("Project")
            let ok (el: XElement) =
                match el.Attribute(proj) with
                | null -> false
                | p when p.Value = targetsRelPath -> true
                | _ -> false
            if doc.Elements(imp) |> Seq.forall (ok >> not) then
                let el = XElement(imp)
                el.SetAttributeValue(proj, targetsRelPath)
                doc.Root.Add(el)
        do // Install lib/net40/*.dll
            let libPaths = Directory.GetFiles(ws.LibDir, "*.dll") |> Array.map getRelPath
            // Remove existing references
            libPaths |> Array.iter (fun p ->
                let asmName = Path.GetFileNameWithoutExtension(p)
                doc.Root.Elements(ns.GetName("ItemGroup")).Descendants(ns.GetName("Reference"))
                |> Seq.tryFind (fun e -> Path.GetFileNameWithoutExtension(e.Value) = asmName)
                |> Option.iter (fun e -> e.Remove()))
            // Remove empty ItemGroups resulting from the above
            doc.Root.Elements(ns.GetName("ItemGroup"))
            |> Seq.filter (fun e -> e.IsEmpty)
            |> Seq.iter (fun e -> e.Remove())
            // Add new references
            let ig = XElement(ns.GetName("ItemGroup"))
            libPaths |> Array.map (fun p ->
                XElement(ns.GetName("Reference"),
                    XAttribute(XName.Get("Include"), Path.GetFileNameWithoutExtension p),
                    XElement(ns.GetName("HintPath"), XText(p)),
                    XElement(ns.GetName("Private"), XText("true"))))
            |> Array.iter ig.Add
            doc.Root.Add ig
        let str = doc.ToString()
        File.WriteAllText(projectFilePath, doc.ToString(), NeutralEncoding)
        CopyTextFile projectFilePath projectFilePath // I assume this fixes line endings?

    let InstallNuGet (nuget: NuGetSource) =
        let getPackage publicName = function
            | PkgLatestPublic ->
                FsNuGet.Package.GetLatest(publicName)
            | PkgBytes bytes ->
                FsNuGet.Package.FromBytes(bytes)
        let wsRoot =
            nuget.WebSharperNuGetPackage |> Option.map (fun wsPkg ->
                let ws = getPackage "WebSharper"  wsPkg
                let wsRoot = Path.Combine(nuget.PackagesDirectory, ws.Text)
                ws.Install(wsRoot)
                wsRoot)
        let wsTpl = getPackage "WebSharper.Templates" nuget.WebSharperTemplatesNuGetPackage
        wsRoot, wsTpl.DataStream

    let CreateLocalSource (wsRoot: string option) wsTpl =
        {
            FileSet = FileSet.FromZip(wsTpl, subdirectory = "templates")
            TargetsFile = Path.Combine(wsRoot.Value, "build", "WebSharper.targets")
            LibDir = Path.Combine(wsRoot.Value, "lib", "net40")
        }

    let InitSource src =
        match src with
        | SLocal local -> local
        | SNuGet nuget ->
            InstallNuGet nuget
            ||> CreateLocalSource

    let Init id opts =
        let opts = Prepare opts
        let local = InitSource opts.Source
        local.FileSet.[id].Populate(opts.Directory)
        MoveProjectFiles opts
        ExpandAllVariables opts

type NuGetPackage with

    static member FromBytes(bytes) =
        Array.copy bytes |> PkgBytes

    static member FromFile(path) =
        File.ReadAllBytes(path) |> PkgBytes

    static member FromStream(s: Stream) =
        ReadStream s |> PkgBytes

    static member LatestPublic() =
        PkgLatestPublic

type Source with
    static member Local(s) = SLocal s
    static member NuGet(s) = SNuGet s

type NuGetSource with

    static member Create() =
        {
            WebSharperNuGetPackage = Some(NuGetPackage.LatestPublic())
            WebSharperTemplatesNuGetPackage = NuGetPackage.LatestPublic()
            PackagesDirectory = "packages"
        }

type InitOptions with

    static member Create() =
        let source = Source.NuGet(NuGetSource.Create())
        {
            Directory = "."
            ProjectName = "MyProject"
            Source = source
        }

type Template with

    member t.Init(opts) =
        match t with
        | T t -> Init t opts

    static member All =
        [
            Template.BundleWebsite
            Template.Extension
            Template.Library
            Template.SiteletsHost
            Template.SiteletsHtml
            Template.SiteletsWebsite
            Template.OwinSelfHost
            Template.BundleUINext
        ]

    static member BundleWebsite = T("bundle-website")
    static member Extension = T("extension")
    static member Library = T("library")
    static member SiteletsHost = T("sitelets-host")
    static member SiteletsHtml = T("sitelets-html")
    static member SiteletsWebsite = T("sitelets-website")
    static member OwinSelfHost = T("owin-selfhost")
    static member BundleUINext = T("bundle-uinext")

