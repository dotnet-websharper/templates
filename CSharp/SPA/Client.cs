using WebSharper;
using WebSharper.UI.Next;
using WebSharper.UI.Next.Client;
using WebSharper.UI.Next.CSharp;
using static WebSharper.UI.Next.CSharp.Client.Html;

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

            new Template.Index.Main()
                .ListContainer(
                    people.View.DocSeqCached((string x) =>
                        new Template.Index.ListItem().Name(x).Doc()
                    )
                )
                .Name(newName)
                .Add(() =>
                {
                    people.Add(newName.Value);
                    newName.Value = "";
                })
                .Doc()
                .RunById("main");
        }
    }
}
