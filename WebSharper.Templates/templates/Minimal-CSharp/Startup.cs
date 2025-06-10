using WebSharper.AspNetCore;
using WebSharper.Min.CSharp;

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

app.UseAuthentication();

app.UseStaticFiles();

app.UseWebSharper(ws => ws.Sitelet(Site.Main));

app.Run();
