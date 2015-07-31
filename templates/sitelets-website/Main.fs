namespace $safeprojectname$

open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About

module Templating =
    open WebSharper.Html.Server

    type Page =
        {
            Title : string
            MenuBar : list<Element>
            Body : list<Element>
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("menubar", fun x -> x.MenuBar)
            .With("body", fun x -> x.Body)

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint =
        let ( => ) txt act =
             LI [if endpoint = act then yield Attr.Class "active"] -< [
                A [Attr.HRef (ctx.Link act)] -< [Text txt]
             ]
        [
            LI ["Home" => EndPoint.Home]
            LI ["About" => EndPoint.About]
        ]

    let Main ctx endpoint title body : Async<Content<EndPoint>> =
        Content.WithTemplate MainTemplate
            {
                Title = title
                MenuBar = MenuBar ctx endpoint
                Body = body
            }

module Site =
    open WebSharper.Html.Server

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            H1 [Text "Say Hi to the server!"]
            Div [ClientSide <@ Client.Main() @>]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            H1 [Text "About"]
            P [Text "This is a template WebSharper client-server application."]
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
        )
