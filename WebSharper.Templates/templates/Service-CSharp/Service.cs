using WebSharper;
using WebSharper.Sitelets;

public class Service
{
    [EndPoint("GET /user")]
    public record GetUser(int id);

    public record User(int Id, string Name);

    [Website]
    public static Sitelet<SPA.EndPoint> Main =>
        new SiteletBuilder()
            .With<GetUser>((ctx, action) =>
                Content.Json (new User (action.id, "John"))
            )
            .Install();
}