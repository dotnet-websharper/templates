using System.Threading.Tasks;
using WebSharper;
using WebSharper.Sitelets;
using WebSharper.UI;
using static WebSharper.UI.Html;

namespace WebSharper.ClientServer.CSharp;

public class Site
{
    [EndPoint("/")]
    public record Home;

    [EndPoint("GET /about")]
    public record About;

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

    public static Task<Content> MainPage(Context<object> ctx, object endpoint) =>
        Content.Page(
            new Template.Main()
                .Title("Home")
                .MenuBar(MenuBar(ctx, endpoint))
                .Body(
                    doc(
                        h1("Say Hi to the server!"),
                        div(client(() => Client.ClientMain()))
                    )
                )
                .Doc(),
            Bundle: "home"
        );

    public static Task<Content> AboutPage(Context<object> ctx, object endpoint) =>
        Content.Page(
            new Template.Main()
                .Title("About")
                .MenuBar(MenuBar(ctx, endpoint))
                .Body(
                    doc(
                        h1("About"),
                        p("This is a template WebSharper client-server application.")
                    )
                )
                .Doc(),
            Bundle: "about"
        );

    [Website]
    public static Sitelet<object> Main =>
        new SiteletBuilder()
            .With<Home>(MainPage)
            .With<About>(AboutPage)
            .Install();
}