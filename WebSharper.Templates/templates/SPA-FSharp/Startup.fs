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

    // Configure the HTTP request pipeline.
    if not (app.Environment.IsDevelopment()) then
        app.UseExceptionHandler("/Error")
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            .UseHsts()
        |> ignore
    
    app.UseHttpsRedirection()
        .UseDefaultFiles()
        .UseStaticFiles()
        .UseWebSharper(fun builder -> builder.UseSitelets(false) |> ignore)
    |> ignore 
       
    app.Run()

    0 // Exit code
