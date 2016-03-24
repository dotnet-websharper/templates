using WebSharper;
using WebSharper.JavaScript;

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
