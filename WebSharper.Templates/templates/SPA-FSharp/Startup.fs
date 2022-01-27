open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open WebSharper.AspNetCore
open WebSharper.SPA.FSharp

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    let app = builder.Build()

    app.UseHttpsRedirection()
        .UseDefaultFiles()
        .UseStaticFiles()
        .UseWebSharper(fun builder -> builder.UseSitelets(false) |> ignore)
        .Run(fun context ->
            context.Response.StatusCode <- 404
            context.Response.WriteAsync("Page not found"))

    0 // Exit code
