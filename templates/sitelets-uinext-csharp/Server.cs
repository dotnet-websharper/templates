using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSharper;
using WebSharper.Sitelets;
using WebSharper.UI.Next;
using WebSharper.UI.Next.CSharp;
using WebSharper.UI.Next.CSharp.Server;
using static WebSharper.UI.Next.CSharp.Html;

namespace $safeprojectname$
{
    public class Server
    {
        [Website]
        public static Sitelet<object> Main =>
            new SiteletBuilder()
                .With<string>((ctx, action) =>
                    Content.Page(
                        Body: doc(
                            h1("My list of unique people"),
                            client(() => Client.Main(action)),
                            button("Clear list").OnClick((el, ev) => Client.ClearNames())
                        )
                    )
                )
                .Install();
    }
}