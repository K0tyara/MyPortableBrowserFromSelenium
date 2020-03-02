using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GoogleChromePortable.Core
{
    class GoogleChrome : IDisposable
    {
        private bool isClosed = false;
        public IWebDriver Driver;
        public GoogleChrome()
        {
            Driver = new LoadWebDriver();
        }

        public Process Run()
        {
            var first = GetProcesses().Select(p => p.Id);

            Driver.Run(GetOrCreateDriverPath());
            var temp = GetProcesses()
                            .Where(p => !first.Contains(p.Id))
                            .ToList();

            temp.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
            var mainProcess = temp.First();
            isClosed = false;

            return mainProcess;
        }
        private List<Process> GetProcesses()
        {
            return Process.GetProcessesByName("chrome")
                                    .ToList();
        }
        public void Close()
        {
            if (isClosed)
                return;

            isClosed = true;
            if (Driver != null)
                Driver.Dispose();
        }
        public void Dispose()
        {
            if (!isClosed)
                Close();
        }
        private string GetOrCreateDriverPath()
        {
            var filePath = $"{Environment.CurrentDirectory}\\browser\\";
            var fileName = "chromedriver.exe";

            if (File.Exists(filePath + fileName))
                return filePath;

            var path = Assembly.GetExecutingAssembly()
                                       .GetManifestResourceNames()
                                       .FirstOrDefault(f => f.EndsWith("chromedriver.exe"));

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            {
                using (var file = new FileStream(filePath + fileName, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(file);
                }
            }
            return filePath;
        }
    }
}
