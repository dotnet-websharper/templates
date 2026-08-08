# Minimal WebSharper F# template

This template provides you with a minimal configuration to start working with WebSharper leveraging ASP.NET Core, using F#.

The application is built using a simple [Sitelet](https://developers.websharper.com/docs/v4.x/fs/sitelets.html). Sitelets are WebSharper's primary way to create server-side content. They provide facilities to route requests and generate HTML pages, or TEXT/JSON responses.

And for this sample, it basically binds the endpoint `GET /` to a controller that outputs the text "Hello World!".

> [!TIP]
> **Learn more**
>
> If this is your first time working with WebSharper, and would like to know more, please read the official documentation:
>
> - Documentation for F#: [link](https://developers.websharper.com/docs/v4.x/fs/overview.html).

## How to run it

1. Make sure that you're into this project's directory at the terminal, then start the server with:

```bash
# After starting the server process, check the logs to know which port it is using.
dotnet run
```

2. Reach the `GET /` endpoint:

```bash
# Change the port according to your environment:
curl localhost:5000/
```

3. Assert that this endpoint returns the string *Hello World!*, with a status code 200. Furthermore, for any other verb and endpoint, the server must respond with a 404 status code response.

## Contribute to this template

If you would like to contribute to this template, please send your patches to [dotnet-websharper/templates](https://github.com/dotnet-websharper/templates), or feel free to open issues at the same repository.