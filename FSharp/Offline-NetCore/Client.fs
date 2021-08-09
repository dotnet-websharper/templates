namespace $safeprojectname$

open WebSharper
open WebSharper.UI
open WebSharper.UI.Notation

[<JavaScript>]
module Client =
    
    type MainTemplate = Templating.Template<"Main.html">

    let DoSomething (input: string) =
        System.String(Array.rev(input.ToCharArray()))

    let Main () =
        let rvReversed = Var.Create ""
        MainTemplate.MainForm()
            .OnSend(fun e ->
                let res = DoSomething e.Vars.TextToReverse.Value
                rvReversed := res
            )
            .Reversed(rvReversed.View)
            .Doc()
