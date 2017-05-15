using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSharper;
using WebSharper.UI.Next;
using WebSharper.UI.Next.Client;
using WebSharper.UI.Next.CSharp;
using WebSharper.UI.Next.CSharp.Client;
using static WebSharper.UI.Next.CSharp.Client.Html;

namespace $safeprojectname$
{
    [JavaScript]
    public static class Client
    {
        static ListModel<string, string> People = new ListModel<string, string>(x => x);
        static Var<string> NewName = Var.Create("");

        static async void InitializeNames()
        {
            foreach (var n in await Remoting.GetNames())
                People.Add(n);
        }

        static public IControlBody Main(string initName)
        {
            NewName.Value = initName;
            InitializeNames();
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

        static public void ClearNames() {
            People.Clear();
        }
    }
}