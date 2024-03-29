using WebSharper;

namespace WebSharper.ClientServer.CSharp;

public static class Remoting
{
    [Remote]
    public static Task<string> DoSomething(string input)
    {
        return Task.FromResult(new String(input.ToCharArray().Reverse().ToArray()));
    }
}