#load "tools/includes.fsx"
open IntelliFactory.Build
open System
open System.IO

let bt =
    BuildTool().PackageId("Zafir.Templates")
        .VersionFrom("Zafir")
        .WithFSharpVersion(FSharpVersion.FSharp31)
        .WithFramework(fun fw -> fw.Net40)

let templates =
    bt.FSharp.Library("WebSharper.Templates")
        .SourcesFromProject()
        .References(fun r ->
            [
                r.Assembly("System.Xml")
                r.Assembly("System.Xml.Linq")
                r.NuGet("SharpCompress").Version("[0.11.6]").ForceFoundVersion().Reference()
            ])

bt.Solution [
    templates
]
|> bt.Dispatch

#r "System.Xml.Linq"
open System.Xml.Linq

if fsi.CommandLineArgs |> Array.contains "skiptests" then
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

    let testDir = __SOURCE_DIRECTORY__ +/ "templates-test"

    let xnm n = XName.Get(n, "http://schemas.microsoft.com/developer/msbuild/2003")
    let xn n = XName.Get(n)

    dirCopy (__SOURCE_DIRECTORY__ +/ "templates") testDir <| fun fn s ->
        let text = s.Replace("$guid1$", Guid.NewGuid().ToString()).Replace("$safeprojectname$", "TemplateTest")
        if fn.EndsWith "proj" then
            let dir = Path.GetFileName(Path.GetDirectoryName(fn))
            let proj = XDocument.Parse(text)
            proj.Root.Add(XElement (xnm"Import", XAttribute(xn"Project", "../../build/net40/templates-test/" + dir + ".proj")))
            string proj.Declaration + string proj
        else 
            text.Replace(
                """    type IndexTemplate = Templating.Template<"index.html">""",
                """    let [<Literal>] ind = __SOURCE_DIRECTORY__ + "/index.html" 
    type IndexTemplate = Templating.Template<ind>"""
            )
    
    // TODO: testing would require inserting an Include for the generated .proj file

    bt.Solution [
        bt.Zafir.BundleWebsite("templates-test/bundle-uinext")
            .SourcesFromProject("UINextApplication.fsproj")
            .References(fun r ->
                [
                    r.NuGet("Zafir.UI.Next").Latest(true).Reference() 
                ])

        bt.Zafir.CSharp.BundleWebsite("templates-test/bundle-uinext-csharp")
            .SourcesFromProject("UINextApplication.csproj")
            .References(fun r ->
                [
                    r.NuGet("Zafir.UI.Next").Latest(true).Reference() 
                ])

        bt.Zafir.BundleWebsite("templates-test/bundle-website")
            .SourcesFromProject("SinglePageApplication.fsproj")

        bt.Zafir.CSharp.BundleWebsite("templates-test/bundle-website-csharp")
            .SourcesFromProject("SinglePageApplication.csproj")

        bt.Zafir.Extension("templates-test/extension")
            .SourcesFromProject("Extension.fsproj")

        bt.Zafir.Library("templates-test/library")
            .SourcesFromProject("Library.fsproj")

        bt.Zafir.CSharp.Library("templates-test/library-csharp")
            .SourcesFromProject("Library.csproj")
    (*
        bt.Zafir.Executable("templates-test/owin-selfhost")
            .SourcesFromProject("SelfHostApplication.fsproj")
            .WithFramework(fun fw -> fw.Net45)
            .References(fun r ->
                [
                    r.NuGet("Microsoft.Owin").Latest(true).Reference() 
                    r.NuGet("Microsoft.Owin.Diagnostics").Latest(true).Reference() 
                    r.NuGet("Microsoft.Owin.FileSystems").Latest(true).Reference() 
                    r.NuGet("Microsoft.Owin.Host.HttpListener").Latest(true).Reference() 
                    r.NuGet("Microsoft.Owin.Hosting").Latest(true).Reference() 
                    r.NuGet("Microsoft.Owin.StaticFiles").Latest(true).Reference() 
                    r.NuGet("Mono.Cecil").Latest(true).Reference() 
                    r.NuGet("Owin").Latest(true).Reference() 
                    r.NuGet("Zafir.Owin").Latest(true).Reference() 
                    r.NuGet("Zafir.Html").Latest(true).Reference() 
                    r.NuGet("IntelliFactory.Xml").Latest(true).Reference() 
                ])
    *)
        bt.Zafir.CSharp.SiteletWebsite("templates-test/sitelets-host")
            .SourcesFromProject("Web.csproj")
            .References(fun r ->
                [
                ])

        bt.Zafir.HtmlWebsite("templates-test/sitelets-html")
            .SourcesFromProject("HtmlApplication.fsproj")

        bt.Zafir.SiteletWebsite("templates-test/sitelets-uinext")
            .SourcesFromProject("UI.Next.Application.fsproj")
            .References(fun r ->
                [
                    r.NuGet("Zafir.UI.Next").Latest(true).Reference() 
                ])

        bt.Zafir.CSharp.SiteletWebsite("templates-test/sitelets-uinext-csharp")
            .SourcesFromProject("UINextApplication.csproj")
            .References(fun r ->
                [
                    r.NuGet("Zafir.UI.Next").Latest(true).Reference() 
                ])

        bt.Zafir.Executable("templates-test/sitelets-uinext-suave")
            .SourcesFromProject("UI.Next.Application.Suave.fsproj")
            .References(fun r ->
                [
                    r.NuGet("Mono.Cecil").Latest(true).Reference() 
                    r.NuGet("Zafir.UI.Next").Latest(true).Reference() 
                    r.NuGet("Zafir.Suave").Latest(true).Reference() 
                    r.NuGet("Suave").Latest(true).Reference() 
                    r.NuGet("Zafir.Owin").Latest(true).Reference() 
                    r.NuGet("Owin").Latest(true).Reference() 
                    r.NuGet("Microsoft.Owin").Latest(true).Reference() 
                ])

        bt.Zafir.CSharp.SiteletWebsite("templates-test/sitelets-website")
            .SourcesFromProject("Application.fsproj")

    ]
    |> bt.Dispatch

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
        |> Array.map (fun fullPath ->
            fullPath, "templates/" + fullPath.[templatesDir.Length + 1 ..].Replace('\\', '/'))
    )
]
|> bt.Dispatch
