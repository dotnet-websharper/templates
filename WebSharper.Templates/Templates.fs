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

type InitOptions =
    {
        Directory : string
        ProjectName : string
        ProjectFileName : string
        TemplatesPackage: byte[]
    }

type Template =
    | T of string

[<AutoOpen>]
module Implementation =

    let UnsafeChars =
        Regex("[^_0-9a-zA-Z.]")

    let CleanIdentifier str =
        match str with
        | null | "" -> "MyProject"
        | str ->
            let str = UnsafeChars.Replace(str, "_")
            if Char.IsDigit(str.[0]) then
                "_" + str
            else
                str

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

    let IsProjectFile path =
        match Path.GetExtension(path) with
        | ".csproj" | ".fsproj" -> true
        | _ -> false

    let MoveProjectFile opts path =
        if IsProjectFile path then
            let ext = Path.GetExtension(path)
            let tgt = Path.Combine(Path.GetDirectoryName(path), opts.ProjectFileName + ext)
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
                    .Replace("$guid1$", FreshGuid ())
                    .Replace("$guid2$", FreshGuid ())
                    .Replace("$safeprojectname$", opts.ProjectName)
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


    let Init id opts =
        EnsureDir opts.Directory
        use stream = new MemoryStream(opts.TemplatesPackage)
        let fileset = FileSet.FromZip(stream, "templates")
        fileset.[id].Populate(opts.Directory)
        MoveProjectFiles opts
        ExpandAllVariables opts

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
            Template.SiteletsUINext
        ]

    static member BundleWebsite = T("bundle-website")
    static member Extension = T("extension")
    static member Library = T("library")
    static member SiteletsHost = T("sitelets-host")
    static member SiteletsHtml = T("sitelets-html")
    static member SiteletsWebsite = T("sitelets-website")
    static member OwinSelfHost = T("owin-selfhost")
    static member BundleUINext = T("bundle-uinext")
    static member SiteletsUINext = T("sitelets-uinext")

type InitOptions with

    static member Create(directory: string, projectName: string, templatesPackage: byte[]) =
        let projectFileName =
            (projectName, Path.GetInvalidFileNameChars())
            ||> Array.fold (fun s c -> s.Replace(c, '_'))
        let projectNameAsIdent = CleanIdentifier projectName
        {
            Directory = directory
            ProjectName = projectNameAsIdent
            ProjectFileName = projectFileName
            TemplatesPackage = templatesPackage
        }
