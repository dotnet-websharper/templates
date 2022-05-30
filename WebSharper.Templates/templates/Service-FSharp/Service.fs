module Service

open WebSharper
open WebSharper.Sitelets

type EndPoint =
    // sample endpoint: /user/1
    | [<EndPoint "GET /user">] GetUser of Id: int

type User =
    { 
        Id: int
        Name: string
    }

[<Website>]
let Main =
    Application.MultiPage (fun ctx endpoint ->
        match endpoint with
        | EndPoint.GetUser uid ->
            Content.Json { Id = uid; Name = "John" }
    )
