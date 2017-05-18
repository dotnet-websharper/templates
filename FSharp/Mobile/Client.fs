namespace $safeprojectname$

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.UI.Next
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Templating

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"index.html", ClientLoad.FromDocument>

    let People = ListModel.Create id []

    [<SPAEntryPoint>]
    let Main () =
        // Replace the URL to where the server is running.
        // Look at for "Project Url" setting on the Web tab of the Properties page of this project.
		WebSharper.Remoting.EndPoint <- "http://localhost:10000/"

        let newName = Var.Create ""

        async {
            try
                let! names = Server.GetNames()    
                names |> List.iter People.Add
            with _ ->
                JS.Alert "Cannot reach server"
        }

        IndexTemplate.Main()
            .ListContainer(
                People.View.DocSeqCached(fun (name: string) ->
                    IndexTemplate.ListItem().Name(name).Doc()
                )
            )
            .Name(newName)
            .Add(fun _ ->
                People.Add(newName.Value)
                newName.Value <- ""
            )
            .Doc()
        |> Doc.RunById "main"
