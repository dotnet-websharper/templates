# Single Page Application WebSharper C# template

This WebSharper template provides you with a simple single page application (SPA) project leveraging ASP.NET using C#.

The application is built using a [SPAEntryPoint](https://developers.websharper.com/docs/v4.x/cs/overview#spa) attribute, exposing a single endpoint for the client to interact (`GET /`).

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

- For a better visualization, please access this page using your browser. You can interact with the input element there, adding new entries to the list of unique people.

## Contribute to this template

If you would like to contribute to this template, please send your patches to [dotnet-websharper/templates](https://github.com/dotnet-websharper/templates), or feel free to open issues at the same repository.