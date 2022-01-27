using WebSharper;
using WebSharper.UI;
using WebSharper.UI.Client;
using static WebSharper.UI.Client.Html;

namespace WebSharper.Offline.CSharp;

[JavaScript]
public static class Client
{
    static ListModel<string, string> People = 
        new ListModel<string, string>(x => x) { "John", "Paul" };

    static Var<string> NewName = Var.Create("");

    static public IControlBody Main()
    {
        return doc(
            ul(People.View.DocSeqCached((string x) => li(x))),
            div(
                input(NewName, attr.placeholder("Name")),
                button("Add", () =>
                {
                    People.Add(NewName.Value);
                    NewName.Value = "";
                }),
                div("You are about to add: ", NewName)
            )
        );
    }

    static public void ClearNames()
	{
        People.Clear();
    }
}
