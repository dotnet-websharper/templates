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
    [Serializable]
    public class ClientControl : WebSharper.Web.Control
    {
        string InitName;

        public ClientControl(string initName)
        {
            InitName = initName;
        }

        [JavaScript]
        override public IControlBody Body {
            get
            {
                var people = ListModel.FromSeq(new[] { "John", "Paul" });
                var newName = Var.Create(InitName);

                return doc(
                    ul(people.View.DocSeqCached((string x) => li(x))),
                    div(
                        input(newName, attr.placeHolder("Name")),
                        button("Add", () =>
                        {
                            people.Add(newName.Value);
                            newName.Value = "";
                        }),
                        div("You are about to add: ", newName.View)
                    )
                );
            }
        }
    }
}