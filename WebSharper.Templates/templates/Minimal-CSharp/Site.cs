using WebSharper;
using WebSharper.Sitelets;
public class Site
{
    [Website]
    public static Sitelet<SPA.EndPoint> Main =>
        Application.Text(ctx => "Hello World");
}