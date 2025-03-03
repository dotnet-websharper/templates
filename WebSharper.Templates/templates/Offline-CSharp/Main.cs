using System.Collections.Generic;
using WebSharper;
using WebSharper.Sitelets;
using WebSharper.UI;
using static WebSharper.UI.Html;

[assembly: Website(typeof(WebSharper.Offline.CSharp.Website))]

namespace WebSharper.Offline.CSharp;

[EndPoint("/")]
public record Home;

[EndPoint("GET /about")]
public record About;

class Website : IWebsite<object>
{
    public IEnumerable<object> Actions =>
        new object[] { new Home(), new About() };

    public static Doc MenuBar(Context<object> ctx, object endpoint)
    {
        Doc link(string txt, object act) =>
            li(
                attr.@class("nav-item"),
                a(attr.href(ctx.Link(act)), attr.@class(endpoint.Equals(act) ? "nav-link active" : "nav-link"), txt)
            );
        return doc(
            li(link("Home", new Home())),
            li(link("About", new About()))
        );
    }

    public static Doc PageContent(Context<object> ctx, object endpoint, string title, Doc body) =>
        new Template.Main()
            .Title(title)
            .MenuBar(MenuBar(ctx, endpoint))
            .Body(body)
            .Doc();

    public Sitelet<object> Sitelet =>
        new SiteletBuilder()
            .With<Home>((ctx, action) =>
                Content.Page(
                    PageContent(ctx, action, "Home",
                        doc(
                            h1("Say Hi to the server!"),
                            div(client(() => Client.ClientMain()))
                        )
                    ),
                    Bundle: "home"
                )
            )
            .With<About>((ctx, action) =>
                Content.Page(
                    PageContent(ctx, action, "About",
                        doc(
                            h1("About"),
                            p("This is a template WebSharper client-server application.")
                        )
                    ),
                    Bundle: "about"
                )
            )
            .Install();
}
