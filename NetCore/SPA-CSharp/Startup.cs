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

namespace WebSharper.SPA.CSharp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSharperRemoting(env)
                .Run(context => {
                    context.Response.StatusCode = 404;
                    return context.Response.WriteAsync("Page not found");
                   });
        }
    }
}
