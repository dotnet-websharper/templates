module Site

open WebSharper
open WebSharper.Sitelets

[<Website>]
let Main = Application.Text (fun ctx -> "Hello World!")