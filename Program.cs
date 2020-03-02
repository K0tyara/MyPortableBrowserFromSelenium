using GoogleChromePortable.Core;
using System;
using System.IO;
using System.Reflection;

namespace GoogleChromePortable
{
    class Program
    {
        private static void CreateDir()
        {
            var path = $"{Environment.CurrentDirectory}\\browser";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        static void Main(string[] args)
        {
            CreateDir();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            using(var google = new GoogleChrome())
            {
                var process = google.Run();
                google.Driver.GoToUrl("chrome://newtab");
                process.WaitForExit();
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
