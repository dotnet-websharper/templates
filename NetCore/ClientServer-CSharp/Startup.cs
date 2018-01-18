using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebSharper.AspNetCore;

namespace WebSharper.ClientServer.CSharp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("WebSharper")
                .AddCookie("WebSharper", options => { });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

            var config =
                new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json")
                    .Build();

            app.UseAuthentication()
                .UseStaticFiles()
                .UseWebSharper(env, Site.Main, config.GetSection("websharper"))
                .Run(context => {
                    context.Response.StatusCode = 404;
                    return context.Response.WriteAsync("Page not found");
                   });
        }
    }
}
