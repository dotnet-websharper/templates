namespace $safeprojectname$

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI.Next
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Html

[<JavaScript>]
module Client =

    let Start input k =
        async {
            let! data = Remoting.Process(input)
            return k data
        }
        |> Async.Start

    let Main () =
        let input = input []
        let label = div []
        div [
            input
            buttonAttr [
                on.click (fun _ _ ->
                    Start input.Value (fun out ->
                        label.Text <- out))
            ] [
                text "Send to the server"
            ]
            label
        ]
