using WebSharper;
using WebSharper.UI;
using WebSharper.UI.Client;
using static WebSharper.UI.Client.Html;

namespace WebSharper.SPA.CSharp
{
    [JavaScript]
    public class App
    {
        [SPAEntryPoint]
        public static void ClientMain()
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
