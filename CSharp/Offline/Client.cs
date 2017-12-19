using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSharper;
using WebSharper.UI;
using WebSharper.UI.Client;
using WebSharper.UI.CSharp;
using WebSharper.UI.CSharp.Client;
using static WebSharper.UI.CSharp.Client.Html;

namespace $safeprojectname$
{
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
}