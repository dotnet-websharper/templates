namespace $safeprojectname$

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.UI.Next
open WebSharper.UI.Next.Notation

[<JavaScript>]
module Client =    
    let [<Literal>] TemplateHtmlPath = __SOURCE_DIRECTORY__ + "/index.html"

    type IndexTemplate = Templating.Template<TemplateHtmlPath> 

    let People =
        ListModel.FromSeq [
            "John"
            "Paul"
        ]

    let Main =
        JQuery.Of("#main").Empty().Ignore

        let newName = Var.Create ""

        IndexTemplate.Main.Doc(
            ListContainer =
                (ListModel.View People |> Doc.Convert (fun name ->
                    IndexTemplate.ListItem.Doc(Name = View.Const name))
                ),
            Name = newName,
            Add = (fun e ->
                People.Add(newName.Value)
                Var.Set newName "")
        )
        |> Doc.RunById "main"
