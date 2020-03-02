using System;

namespace GoogleChromePortable.Core
{
    interface IWebDriver : IDisposable
    {
        void Run(string driverPath);
        void Close();
        void GoToUrl(string url);
    }
}