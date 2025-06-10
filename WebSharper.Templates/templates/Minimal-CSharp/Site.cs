using WebSharper;
using WebSharper.Sitelets;

namespace WebSharper.Min.CSharp;

public class Site
{
    [Website]
    public static Sitelet<SPA.EndPoint> Main =>
        Application.Text(ctx => "Hello World");
}