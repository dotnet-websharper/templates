namespace $safeprojectname$

open WebSharper
open WebSharper.JavaScript
open WebSharper.Html.Client

[<JavaScript>]
module Client =

    let DoSomething (input: string) =
        System.String(Array.rev(input.ToCharArray()))

    let Main () =
        let input = Input [Attr.Value ""] -< []
        let output = H1 []
        Div [
            input
            Button [Text "Send"]
            |>! OnClick (fun _ _ ->
                output.Text <- DoSomething input.Value
            )
            HR []
            H4 [Attr.Class "text-muted"] -< [Text "The server responded:"]
            Div [Attr.Class "jumbotron"] -< [output]
        ]
