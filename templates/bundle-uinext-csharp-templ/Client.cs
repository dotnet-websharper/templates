using WebSharper;
using WebSharper.JQuery;
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
            JQuery.Of("#main").Empty();

            var people = ListModel.FromSeq(new[] { "John", "Paul" });
            var newName = Var.Create("");

            Template.Index.Main.Doc(
                ListContainer:
                    people.View.DocSeqCached(name => 
                        Template.Index.ListItem.Doc(Name: View.Const(name))
                    ),
                Name: newName,
                Add: (el, ev) =>
                {
                    people.Add(newName.Value);
                    newName.Value = "";
                }
            ).RunById("main");
        }
    }
}
