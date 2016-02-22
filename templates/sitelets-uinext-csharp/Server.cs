using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSharper;
using WebSharper.Sitelets;
using WebSharper.UI.Next;
using WebSharper.UI.Next.CSharp;
using static WebSharper.UI.Next.CSharp.Html;

namespace $safeprojectname$
{
    public class Server
    {
        [Website]
        public static Sitelet<object> Main =>
            new SiteletBuilder()
                .With<string>((ctx, action) =>
                    Content.Page<string>(
                        Body: doc(
                            h1("My list of unique people"),
                            new ClientControl(action)
                        )
                    )
                )
                .Install();
    }
}