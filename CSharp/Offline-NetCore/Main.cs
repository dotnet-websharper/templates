using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.FSharp.Collections;
using WebSharper;
using WebSharper.Sitelets;
using WebSharper.UI;
using static WebSharper.UI.Html;

[assembly: Website(typeof($safeprojectname$.Website))]
namespace $safeprojectname$
{
    [EndPoint("/")]
    public record Home;

    [EndPoint("GET /about")]
    public record About;

    class Website : IWebsite<object>
    {
        public FSharpList<object> Actions =>
            FSharpConvert.List<object>(new Home(), new About());

        public Sitelet<object> Sitelet =>
            new SiteletBuilder()
                .With<Home>((ctx, action) =>
                    Content.Page(
                        Body: doc(
                            h1("My list of unique people"),
                            client(() => Client.Main()),
                            button("Clear list", on.click((el, ev) => Client.ClearNames())),
                            div(a(attr.href(ctx.Link(new About())), "About"))
                        )
                    )
                )
                .With<About>((ctx, action) =>
                    Content.Page(
                        Body: doc(
                            h1("About page"),
                            div("This is a simple WebSharper example on working with a reactive list model."),
                            div(a(attr.href(ctx.Link(new Home())), "Home"))
                        )
                    )
                )
                .Install();
    }
}