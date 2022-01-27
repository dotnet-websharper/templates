open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open WebSharper.AspNetCore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    builder.Services.AddSitelet(Site.Main)
        .AddAuthentication("WebSharper")
        .AddCookie("WebSharper", fun options -> ())
    |> ignore

    let app = builder.Build()

    app.UseHttpsRedirection()
        .UseAuthentication()
        .UseStaticFiles()
        .UseWebSharper()
        .Run(fun context ->
            context.Response.StatusCode <- 404
            context.Response.WriteAsync("Page not found"))

    0 // Exit code
