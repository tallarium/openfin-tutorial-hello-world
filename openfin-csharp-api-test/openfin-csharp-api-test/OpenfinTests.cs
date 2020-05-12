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
    class FirstTest
    {
        ChromeDriver driver;

        public void StartOpenfinApp()
        {
            var options = new ChromeOptions();
            string dir = Path.GetDirectoryName(GetType().Assembly.Location);
            options.BinaryLocation = Path.Combine(dir, "RunOpenFin.bat");
            options.AddArgument("--config=http://localhost:9070/app.json");
            options.AddArgument("--remote-debugging-port=4444");
            driver = new ChromeDriver(options);
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
            
            Runtime runtime = await ConnectToRuntime();
            Application app = runtime.WrapApplication("openfin-closing-events-demo");
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

            Runtime runtime = await ConnectToRuntime();
            Application app = runtime.WrapApplication("openfin-closing-events-demo");

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
            }
            driver = null;
        }
    }
}
