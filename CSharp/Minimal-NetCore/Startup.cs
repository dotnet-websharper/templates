using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSharper.AspNetCore;

namespace WebSharper.Min.CSharp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSitelet(Site.Main)
                .AddAuthentication("WebSharper")
                .AddCookie("WebSharper", options => { });
        }

        public void Configure(IApplicationBuilder app, $if$ ($visualstudioversion$ >= 16.0)IWebHostEnvironment$else$IHostingEnvironment$endif$ env)
        {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

            app.UseAuthentication()
                .UseStaticFiles()
                .UseWebSharper()
                .Run(context =>
                {
                    context.Response.StatusCode = 404;
                    return context.Response.WriteAsync("Page not found");
                });
        }

        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
