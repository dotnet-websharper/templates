namespace WebSharper.ClientServer.FSharp

open WebSharper
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation
[<JavaScript>]
module Client =

    type MainTemplate = Templating.Template<"Main.html">

    let Main () =
        let rvReversed = Var.Create ""
        MainTemplate.MainForm()
            .OnSend(fun e ->
                async {
                    let! res = Server.DoSomething e.Vars.TextToReverse.Value
                    rvReversed := res
                }
                |> Async.StartImmediate
            )
            .Reversed(rvReversed.View)
            .Doc()
