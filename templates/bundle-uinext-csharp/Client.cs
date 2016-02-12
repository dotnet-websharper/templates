using WebSharper.UI.Next;
using WebSharper.UI.Next.Client;
using WebSharper.UI.Next.CSharp.Extensions;
using static WebSharper.UI.Next.CSharp.Html;
using static WebSharper.Core.Attributes;

using D = WebSharper.UI.Next.Client.Doc;

namespace $safeprojectname$
{
    [JavaScript]
    public class App
    {
        [SPAEntryPoint]
        public static void Main()
        {
            var people = ListModel.FromSeq(new[] { "John", "Paul" });
            var newName = Var.Create("");

            div(
                h1("My list of unique people"),
                ul(people.View.DocSeqCached((string x) => li(x))),
                div(
                    D.Input(new[] { attr.placeholder("Name") }, newName),
                    div(newName.View)
                )
            ).RunById("main");
        }
    }
}
