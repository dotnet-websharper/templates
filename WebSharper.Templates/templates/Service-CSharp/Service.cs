using WebSharper;
using WebSharper.Sitelets;

public class Service
{
    // sample endpoint: /user/1
    [EndPoint("GET /user/{Id}")]
    public record GetUser(int Id);

    public record User(int Id, string Name);

    [Website]
    public static Sitelet<object> Main =>
        new SiteletBuilder()
            .With<GetUser>((ctx, action) =>
                Content.Json(new User(action.Id, "John"))
            )
            .Install();
}