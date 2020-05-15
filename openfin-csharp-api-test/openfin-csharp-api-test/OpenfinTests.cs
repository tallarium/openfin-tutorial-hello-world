using NUnit.Framework;
using Openfin.Desktop;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        Runtime runtime;

        public void StartOpenfinApp()
        {
            string dir = Path.GetDirectoryName(GetType().Assembly.Location);

            var service = ChromeDriverService.CreateDefaultService();
            service.LogPath = Path.Combine(dir, "chromedriver.log");
            service.EnableVerboseLogging = true;

            var options = new ChromeOptions();

            string runOpenfinPath = Path.Combine(dir, "RunOpenFin.bat");
            string appConfigArg = "--config=http://localhost:9070/app.json";
            if (runtime != null)
            {
                options.DebuggerAddress = "localhost:4444";
                Process.Start(runOpenfinPath, appConfigArg);
            } else
            {
                options.BinaryLocation = runOpenfinPath;
                options.AddArgument(appConfigArg);
                options.AddArgument("--remote-debugging-port=4444");
            }
            driver = new ChromeDriver(service, options);
        }

        private Task<Runtime> ConnectToRuntime()
        {
            var taskCompletionSource = new TaskCompletionSource<Runtime>();

            RuntimeOptions options = new RuntimeOptions();
            options.Version = "14.78.46.23";
            options.Arguments = "--remote-debugging-port=4444";
            runtime = Openfin.Desktop.Runtime.GetRuntimeInstance(options);
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
                taskCompletionSource.SetResult(isRunning);
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

        [Test]
        public async Task AppEventsInitiallyClosed()
        {
            bool startedFired = false;
            bool closedFired = false;

            Application app = await GetApplication(OPENFIN_APP_UUID);
            app.Started += (object sender, ApplicationEventArgs e) =>
            {
                startedFired = true;
            };

            app.Closed += (object sender, ApplicationEventArgs e) =>
            {
                closedFired = true;
            };

            StartOpenfinApp();
            Assert.IsTrue(startedFired);
            StopOpenfinApp();
            Assert.IsTrue(closedFired);
        }

        [Test]
        public async Task AppEventsInitiallyOpen()
        {
            bool startedFired = false;
            bool closedFired = false;

            StartOpenfinApp();

            Application app = await GetApplication(OPENFIN_APP_UUID);
            app.Started += (object sender, ApplicationEventArgs e) =>
            {
                startedFired = true;
            };

            app.Closed += (object sender, ApplicationEventArgs e) =>
            {
                closedFired = true;
            };

            StopOpenfinApp();
            Assert.IsTrue(closedFired);
            StartOpenfinApp();
            Assert.IsTrue(startedFired);
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
