using WebSharper;
using WebSharper.UI;
using static WebSharper.UI.Templating.AST;

namespace WebSharper.Offline.CSharp;

[JavaScript]
public static class Client
{
    static public string DoSomething(string input)
    {
        return new string(input.ToCharArray().Reverse().ToArray());
    }

    static public IControlBody ClientMain()
    {
        var vReversed = Var.Create("");
        return new Template.Main.MainForm()
            .Reversed(vReversed.View)
            .OnSend(e => {
                var rev = DoSomething(e.Vars.TextToReverse.Value);
                vReversed.Set(rev);
            })
            .Doc();
    }
}