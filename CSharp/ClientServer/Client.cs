using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebSharper;
using WebSharper.UI.Next;
using WebSharper.UI.Next.Client;
using WebSharper.UI.Next.CSharp;
using WebSharper.UI.Next.CSharp.Client;
using Microsoft.FSharp.Core;
using static WebSharper.UI.Next.CSharp.Client.Html;

namespace $safeprojectname$
{
    [JavaScript]
    public static class Client
    {
        static public IControlBody Main()
        {
            var rvInput = Var.Create("");
            var submit = Submitter.CreateOption(rvInput.View);
            var vReversed =
                submit.View.MapAsync(input =>
                {
                    if (input is null)
                        return Task.FromResult("");
                    return Remoting.DoSomething(input.Value);
                }
                );
            return div(
                input(rvInput),
                button("Send", submit.Trigger),
                hr(),
                h4(
                    attr.@class("text-muted"),
                    "The server responded:",
                    div(
                        attr.@class("jumbotron"),
                        h1(vReversed)
                    )
                )
            );
        }
    }
}