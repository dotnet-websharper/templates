# Client-Server WebSharper C# template

This WebSharper template provides you with a simple project with client and server components leveraging ASP.NET using C#.

The [multi-page application](https://developers.websharper.com/docs/v4.x/fs/overview#heading-9) is built using a [Sitelet](https://developers.websharper.com/docs/v4.x/cs/sitelets.html) exposing primarily two front-end endpoints (`GET /` and `GET /about`) and one back-end endpoint (`POST Server/DoSomething`). Sitelets are WebSharper's primary way to create server-side content. They provide facilities to route requests and generate HTML pages, or TEXT/JSON responses.

Notice that the back-end endpoint is configured using the Rpc attribute. If you want to get a better understanding of this feature, please check the [Remoting documentation](https://developers.websharper.com/docs/v4.x/cs/remoting).

> [!TIP]
> **Learn more**
>
> If this is your first time working with WebSharper, and would like to know more, please read the official documentation:
>
> - Documentation for C#: [link](https://developers.websharper.com/docs/v4.x/cs/overview.html).

## How to run it

1. Make sure that you're into this project's directory at the terminal, then start the server with:

```bash
# After starting the server process, check the logs to know which port it is using.
dotnet run
```

2. Access the Home page (`GET /` endpoint):

```bash
# Change the port according to your environment:
curl localhost:5000/
```

- For a better visualization, please access this page using your browser.

3. Access the About page (`GET /about` endpoint):

```bash
# Change the port according to your environment:
curl localhost:5000/about
```

- For a better visualization, please access this page using your browser.

4. If you want to test the back-end endpoint (`POST /Server/DoSomething`), you can also use curl:

```bash
curl 'http://localhost:5000/Server/DoSomething' \
  -H 'Origin: http://localhost:5000' \
  --data-raw '["Hello World!"]'
# result:
# "!dlroW olleH"
```

## Contribute to this template

If you would like to contribute to this template, please send your patches to [dotnet-websharper/templates](https://github.com/dotnet-websharper/templates), or feel free to open issues at the same repository.