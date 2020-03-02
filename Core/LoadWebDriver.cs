using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;

namespace GoogleChromePortable.Core
{
    class LoadWebDriver : CriticalFinalizerObject, IWebDriver, IDisposable
    {
        private readonly Assembly webDriverAssem;
        private object driver;
        public LoadWebDriver()
        {
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
        }

        public void Run(string driverPath)
        {
            var driver_type = webDriverAssem
                           .ExportedTypes
                           .FirstOrDefault(t => t.Name.EndsWith("ChromeDriver"));

            var options = GetOptions();
            var service = GetService(driverPath);
            var constuctor = driver_type.GetConstructor(new[]
            {
                service.GetType(),
                options.GetType(),
                typeof(TimeSpan)
            });
            driver = constuctor.Invoke(new[] { service, options, TimeSpan.FromSeconds(30) });
        }
        public void Close()
        {
            driver.GetType().GetMethod("Dispose").Invoke(driver, null);
        }
        public void Dispose()
        {
            Close();
        }
        public void GoToUrl(string url)
        {
            var navigation = driver.GetType().GetMethod("Navigate").Invoke(driver, null);
            navigation.GetType().GetMethod("GoToUrl", new[] { typeof(string) })
                                .Invoke(navigation, new[] { url });
        }
        private object GetService(string driverPath)
        {
            var service_type = webDriverAssem
                               .ExportedTypes
                               .FirstOrDefault(f => f.Name.EndsWith("ChromeDriverService"));

            var service = service_type.GetMethod("CreateDefaultService", new[] { typeof(string) })
                                      .Invoke(null, new[] { driverPath });
            service_type.GetProperty("HideCommandPromptWindow").SetValue(service, true);

            return service;
        }
        private object GetOptions()
        {
            var options_type = webDriverAssem
                               .ExportedTypes
                               .FirstOrDefault(f => f.Name.EndsWith("ChromeOptions"));
            var args = new[]
            {
                "--disable-notifications",
                "--force-android-app-mode",
                "--use-fake-ui-for-media-stream",
                $"user-data-dir={Environment.CurrentDirectory}\\browser"
            };
            var options = Activator.CreateInstance(options_type);
            options_type.GetMethod("AddArguments", new[] { typeof(string[]) }).Invoke(options, new[] { args });

            var mobile = GetMobileOptions();
            options_type.GetMethod("EnableMobileEmulation", new[] { mobile.GetType() }).Invoke(options, new[] { mobile });

            return options;
        }
        private object GetMobileOptions()
        {
            var mobile_type = webDriverAssem
                              .ExportedTypes
                              .FirstOrDefault(t => t.Name.EndsWith("ChromeMobileEmulationDeviceSettings"));

            var mobile = Activator.CreateInstance(mobile_type);
            mobile_type.GetProperty("EnableTouchEvents").SetValue(mobile, true);

            var userAgent = "Mozilla/5.0 (iPad; CPU OS 11_0 like Mac OS X) AppleWebKit/604.1.34 (KHTML, like Gecko) Version/11.0 Mobile/15A5341f Safari/604.1";
            mobile_type.GetProperty("UserAgent").SetValue(mobile, userAgent);

            return mobile;
        }
    }
}
