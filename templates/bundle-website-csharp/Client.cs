using WebSharper;
using WebSharper.JavaScript;
using static WebSharper.Core.Attributes;

namespace $safeprojectname$
{
    public class Client
    {
        [JavaScript]
        [SPAEntryPoint]
        public static void Main()
        {
            Console.Log("Running JavaScript Entry Point..");
        }
    }
}