using WebSharper;
using WebSharper.Sitelets;

public class Service
{
    [EndPoint("/")]
    public record Home;

    // sample endpoint: /user/1
    [EndPoint("GET /user/{Id}")]
    public record GetUser(int Id);

    public record User(int Id, string Name);

    [Website]
    public static Sitelet<object> Main =>
        new SiteletBuilder()
            .With<Home>((ctx, _) => Content.Text("Service version 1.0"))
            .WithCors<GetUser>(
                corsBuilder => corsBuilder.WithOrigins("http://example.com"),
                (ctx, action) => Content.Json(new User(action.Id, "John"))
            )
            .Install();
}