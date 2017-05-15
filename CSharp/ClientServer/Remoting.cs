using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebSharper;

namespace $safeprojectname$
{
    public static class Remoting
    {
        [Remote]
        public static Task<string[]> GetNames()
        {
            return Task.FromResult(new[] { "John", "Paul" });
        }
    }
}