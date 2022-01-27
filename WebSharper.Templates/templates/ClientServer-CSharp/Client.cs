using WebSharper;
using WebSharper.UI;

namespace WebSharper.ClientServer.CSharp;

[JavaScript]
public static class Client
{
    static public IControlBody ClientMain()
    {
        var vReversed = Var.Create("");
        return new Template.Main.MainForm()
            .Reversed(vReversed.View)
            .OnSend(async e => {
                var rev = await Remoting.DoSomething(e.Vars.TextToReverse.Value);
                vReversed.Set (rev);
            })
            .Doc();
    }
}