using Microsoft.FSharp.Core;
using System;
using WebSharper;
using WebSharper.UI.Next;
using static WebSharper.Core.Attributes;
using static WebSharper.UI.Next.Client.DocExtensions;
using Dom = WebSharper.JavaScript.Dom;
using JSConsole = WebSharper.JavaScript.Console;

namespace $safeprojectname$
{
    [JavaScript]
    public class App
    {
        [Inline("function (x) { $wsruntime.InvokeDelegate($f, [x]) }")]
        private static FSharpFunc<T, Unit> ConvertMethod<T>(Action<T> f) => null;

        private static FSharpFunc<T1, FSharpFunc<T2, Unit>> ConvertMethod<T1, T2>(Action<T1, T2> f)
            => ConvertMethod((T1 x) => ConvertMethod((T2 y) => f(x, y)));

        [Inline("function (x) { return $wsruntime.InvokeDelegate($f, [x]) }")]
        private static FSharpFunc<T, U> ConvertMethod<T, U>(Func<T, U> f) => null;

        private static FSharpFunc<T1, FSharpFunc<T2, U>> ConvertMethod<T1, T2, U>(Func<T1, T2, U> f)
            => ConvertMethod((T1 x) => ConvertMethod((T2 y) => f(x, y)));

        [SPAEntryPoint]
        public static void Main()
        {
            var name = Var.Create("...");

            WebSharper.UI.Next.Client.Doc.RunById("main",
                Html.div(new[] {
                    Html.div( new[] { Html.text("Hello "), Html.textView(name.View) }),
                    Html.button (new [] { Html.text("Click me!") } )
                        .OnClick(ConvertMethod((Dom.Element el, Dom.MouseEvent ev) => { name.Value = "world!"; }))
                })
            );
        }
    }
}
