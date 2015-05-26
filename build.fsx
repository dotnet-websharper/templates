#load "tools/includes.fsx"
open IntelliFactory.Build
open System.IO

let bt =
    BuildTool().PackageId("WebSharper.Templates")
        .VersionFrom("WebSharper")

let templates =
    bt.WithFramework(bt.Framework.Net40)
        .FSharp.Library("WebSharper.Templates")
        .SourcesFromProject()
        .References(fun r ->
            [
                r.Assembly("System.Xml")
                r.Assembly("System.Xml.Linq")
                r.NuGet("FsNuget").Reference()
                r.NuGet("SharpCompress").Reference()
            ])

bt.Solution [
    templates

    bt.NuGet.CreatePackage()
        .Configure(fun c ->
            { c with
                Title = Some "WebSharper.Templates"
                LicenseUrl = Some "http://websharper.com/licensing"
                ProjectUrl = Some "https://github.com/intellifactory/websharper.visualstudio"
                Description = "WebSharper Project Templates"
                RequiresLicenseAcceptance = true })
        .Add(templates)
    |> Array.foldBack (fun f n -> n.AddFile(f)) (
        let templatesDir = DirectoryInfo("templates").FullName
        Directory.GetFiles(templatesDir, "*", SearchOption.AllDirectories)
        |> Array.map (fun fullPath ->
            fullPath, "templates/" + fullPath.[templatesDir.Length + 1 ..].Replace('\\', '/'))
    )
]
|> bt.Dispatch
