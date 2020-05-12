using NUnit.Framework;
using Openfin.Desktop;
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
    class OpenfinTests
    {
        private const string OPENFIN_APP_UUID = "openfin-closing-events-demo";

        ChromeDriver driver;

        public void StartOpenfinApp()
        {
            string dir = Path.GetDirectoryName(GetType().Assembly.Location);

            var service = ChromeDriverService.CreateDefaultService();
            service.LogPath = Path.Combine(dir, "chromedriver.log");
            service.EnableVerboseLogging = true;

            var options = new ChromeOptions();
            options.BinaryLocation = Path.Combine(dir, "RunOpenFin.bat");
            options.AddArgument("--config=http://localhost:9070/app.json");
            options.AddArgument("--remote-debugging-port=4444");
            driver = new ChromeDriver(service, options);
        }

        private Task<Runtime> ConnectToRuntime()
        {
            var taskCompletionSource = new TaskCompletionSource<Runtime>();

            RuntimeOptions options = new RuntimeOptions();
            options.Version = "14.78.46.23";
            Runtime runtime = Openfin.Desktop.Runtime.GetRuntimeInstance(options);
            runtime.Connect(() =>
            {
                taskCompletionSource.SetResult(runtime);
            });

            return taskCompletionSource.Task;
        }

        private async Task<Application> GetApplication(string UUID)
        {
            Runtime runtime = await ConnectToRuntime();
            return runtime.WrapApplication(UUID);
        }

        private Task<bool> AppIsRunning(Application app)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            app.isRunning((Ack ack) =>
            {
                bool isRunning = ack.getJsonObject().Value<bool>("data");
                taskCompletionSource.TrySetResult(isRunning);
            }, (Ack ack) =>
            {
                // Error
            });
            return taskCompletionSource.Task;
        }

        [Test]
        public void Test()
        {
            StartOpenfinApp();
            driver.ExecuteScript("alert('My first test')");
        }

        // TODO: This test fails as ChromeDriver fails to connect to OpenFin
        [Test]
        public async Task IsRunningInitiallyClosed()
        {

            Application app = await GetApplication(OPENFIN_APP_UUID);
            bool isRunning = await AppIsRunning(app);

            Assert.IsFalse(isRunning);
            StartOpenfinApp();
            isRunning = await AppIsRunning(app);
            Assert.IsTrue(isRunning);
            StopOpenfinApp();
            isRunning = await AppIsRunning(app);
            Assert.IsFalse(isRunning);
        }

        [Test]
        public async Task IsRunningInitiallyOpen()
        {

            StartOpenfinApp();

            Application app = await GetApplication(OPENFIN_APP_UUID);

            bool isRunning = await AppIsRunning(app);

            Assert.IsTrue(isRunning);
            StopOpenfinApp();
            isRunning = await AppIsRunning(app);
            Assert.IsFalse(isRunning);
            StartOpenfinApp();
            isRunning = await AppIsRunning(app);
            Assert.IsTrue(isRunning);
        }

        [TearDown]
        public void StopOpenfinApp()
        {
            if (driver != null)
            {
                // Neither of the below actually close the OpenFin runtime
                //driver.Close();
                //driver.Quit();
                driver.ExecuteScript("window.close()"); // This does
                driver.Quit();
            }
            driver = null;
        }
    }
}
