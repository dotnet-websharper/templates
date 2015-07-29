namespace $safeprojectname$

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About

module Skin =

    type MainTemplate = Templating.Template<"Main.html">

    let WithTemplate title body =
        Content.Doc(
            MainTemplate.Doc(
                title = title,
                body = body
            )
        )

module Site =
    open WebSharper.UI.Next.Html

    let ( ==> ) txt url =
        aAttr [ attr.href url ] [ text txt ]

    let Links (ctx: Context<EndPoint>) =
        ul
          [ li [ "Home" ==> ctx.Link Home ]
            li [ "About" ==> ctx.Link About ] ]

    let HomePage ctx =
        Skin.WithTemplate "HomePage"
          [ div [ text "HOME" ]
            div [ client <@ Client.Main() @> ]
            Links ctx ]

    let AboutPage ctx =
        Skin.WithTemplate "AboutPage"
          [ div [ text "ABOUT" ]
            Links ctx ]

    [<Website>]
    let Main = Sitelet.Infer <| fun ctx endpoint ->
        match endpoint with
        | Home -> HomePage ctx
        | About -> AboutPage ctx
