namespace $safeprojectname$

open WebSharper.Html.Server
open WebSharper
open WebSharper.Sitelets

type Action =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /about">] About

module Skin =
    open System.Web

    type Page =
        {
            Title : string
            Body : list<Element>
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("body", fun x -> x.Body)

    let WithTemplate title body : Async<Content<Action>> =
        Content.WithTemplate MainTemplate
            {
                Title = title
                Body = body
            }

module Site =

    let ( => ) text url =
        A [HRef url] -< [Text text]

    let Links (ctx: Context<Action>) =
        UL [
            LI ["Home" => ctx.Link Home]
            LI ["About" => ctx.Link About]
        ]

    let HomePage ctx =
        Skin.WithTemplate "HomePage"
            [
                Div [Text "HOME"]
                Div [ClientSide <@ Client.Main() @>]
                Links ctx
            ]

    let AboutPage ctx =
        Skin.WithTemplate "AboutPage"
            [
                Div [Text "ABOUT"]
                Links ctx
            ]

    [<Website>]
    let Main =
        Sitelet.Infer <| fun ctx action ->
            match action with
            | Home -> HomePage ctx
            | About -> AboutPage ctx

[<Sealed>]
type Website() =
    interface IWebsite<Action> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home; About]

[<assembly: Website(typeof<Website>)>]
do ()
