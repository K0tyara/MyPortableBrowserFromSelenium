using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;

namespace GoogleChromePortable.Core
{
    class WebDriver : CriticalFinalizerObject, IWebDriver, IDisposable
    {
        private readonly Assembly driverAssembly;
        private object driver;
        public WebDriver()
        {
            driverAssembly = LoadAssembly.LoadWebDriver();
        }

        private Action<string> goToUrl = (s) => { };

        public void Run(string driverPath)
        {
            var driver_type = driverAssembly
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

            object navigation = ((dynamic)driver).Navigate();

            var mi = navigation.GetType().GetTypeInfo().GetMethod("GoToUrl", new[] { typeof(string) });

            goToUrl = mi.CreateDelegate<Action<string>>(navigation);
        }
        public void Close()
        {
            (driver as IDisposable).Dispose();
        }
        public void Dispose()
        {
            Close();
        }
        public void GoToUrl(string url)
        {
            goToUrl.Invoke(url);
        }
        private object GetService(string driverPath)
        {
            var service_type = driverAssembly
                               .ExportedTypes
                               .FirstOrDefault(f => f.Name.EndsWith("ChromeDriverService"));

            dynamic service = service_type.GetMethod("CreateDefaultService", new[] { typeof(string) })
                                      .Invoke(null, new[] { driverPath });
            service.HideCommandPromptWindow = true;

            return service;
        }
        private object GetOptions()
        {
            var options_type = driverAssembly
                               .ExportedTypes
                               .FirstOrDefault(f => f.Name.EndsWith("ChromeOptions"));
            var args = new[]
            {
                "--disable-notifications",
                "--force-android-app-mode",
                "--use-fake-ui-for-media-stream",
                $"user-data-dir={Environment.CurrentDirectory}\\browser"
            };
            dynamic options = Activator.CreateInstance(options_type);
            options.AddArguments(args);

            dynamic mobile = GetMobileOptions();
            options.EnableMobileEmulation(mobile);

            return options;
        }
        private object GetMobileOptions()
        {
            var mobile_type = driverAssembly
                              .ExportedTypes
                              .FirstOrDefault(t => t.Name.EndsWith("ChromeMobileEmulationDeviceSettings"));

            dynamic mobile = Activator.CreateInstance(mobile_type);
            mobile.EnableTouchEvents = true;
            mobile.UserAgent = "Mozilla/5.0 (iPad; CPU OS 11_0 like Mac OS X) AppleWebKit/604.1.34 (KHTML, like Gecko) Version/11.0 Mobile/15A5341f Safari/604.1";

            return mobile;
        }
    }
}
