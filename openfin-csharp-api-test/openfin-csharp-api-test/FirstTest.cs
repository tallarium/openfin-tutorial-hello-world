using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenfinDesktop
{
    class FirstTest
    {
        ChromeDriver driver;

        [SetUp]
        public void StartBrowser()
        {
            var options = new ChromeOptions();
            string dir = Path.GetDirectoryName(GetType().Assembly.Location);
            options.BinaryLocation = Path.Combine(dir, "RunOpenFin.bat");
            options.AddArgument("--config=http://localhost:9070/app.json");
            options.AddArgument("--remote-debugging-port=4444");
            driver = new ChromeDriver(options);
        }

        [Test]
        public void Test()
        {
            driver.ExecuteScript("alert('My first test')");
        }

        [TearDown]
        public void CloseBrowser()
        {
            // Neither of the below actually close the OpenFin runtime
            //driver.Close();
            //driver.Quit();
            driver.ExecuteScript("window.close()"); // This does
        }
    }
}
