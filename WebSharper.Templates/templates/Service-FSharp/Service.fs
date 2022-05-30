module Service

open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "GET /user">] GetUser of int: id

type User =
    { 
        Id: int
        Name: int
    }

[<Website>]
let Main =
    Application.MultiPage (fun ctx endpoint ->
        match endpoint with
        | EndPoint.GetUser uid ->
            Content.Json { Id = uid; Name = "John" }
    )
