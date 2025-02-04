﻿using WebSharper.AspNetCore;
using WebSharper.ClientServer.CSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddWebSharper()
                .AddAuthentication("WebSharper")
                .AddCookie("WebSharper", options => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//-:cnd:noEmit
#if DEBUG        
app.UseWebSharperScriptRedirect(startVite: true);
#endif
//+:cnd:noEmit

app.UseAuthentication();

app.UseStaticFiles();

app.UseWebSharper(ws => ws.Sitelet(Site.Main));

app.Run();
