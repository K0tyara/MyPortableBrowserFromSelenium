using System.IO;
using System.Linq;
using System.Reflection;

namespace GoogleChromePortable.Core
{
    class LoadAssembly
    {
        public static Assembly LoadWebDriver()
        {
            Assembly webDriverAssem = null;
            var currAssem = Assembly.GetExecutingAssembly();
            var file = currAssem.GetManifestResourceNames()
                            .FirstOrDefault(f => f.EndsWith("WebDriver.dll"));
            if (file == null)
                throw new FileNotFoundException("WebDriver.dll not found.");

            using (var stream = currAssem.GetManifestResourceStream(file))
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);

                webDriverAssem = Assembly.Load(data);
            }

            return webDriverAssem;
        }
    }
}
