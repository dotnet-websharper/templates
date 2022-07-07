module Service

open WebSharper
open WebSharper.Sitelets

type EndPointWithCors =
    | [<EndPoint "GET /user">] GetUser of Id: int 

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/">] EndPointWithCors of Cors<EndPointWithCors>

type User =
    { 
        Id: int
        Name: string
    }

let HandleApi ctx endpoint =
    match endpoint with
    | GetUser uid ->
        Content.Json { Id = uid; Name = "John" }

[<Website>]
let Main =
    Application.MultiPage (fun ctx endpoint ->
        match endpoint with
        | Home -> Content.Text "Service version 1.0"
        | EndPointWithCors endpoint ->
            Content.Cors endpoint 
                (fun corsAllows ->
                    { corsAllows with
                        Origins = ["http://example.com"]
                    }
                )
                (HandleApi ctx)
    )

