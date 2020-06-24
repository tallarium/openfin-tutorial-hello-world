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
        private const string OPENFIN_APP_UUID = "openfin-tests";

        private const string OPENFIN_ADAPTER_RUNTIME = "14.78.46.23";
        private string OPENFIN_APP_RUNTIME = "";

        private bool shareRuntime
        {
            get => OPENFIN_APP_RUNTIME == OPENFIN_ADAPTER_RUNTIME;
        }

        private const int FILE_SERVER_PORT = 9070;
        private const int REMOTE_DEBUGGING_PORT = 4444;

        private static readonly string FILE_SERVER_ROOT_URL = String.Format("http://localhost:{0}/", FILE_SERVER_PORT);
        private static readonly string APP_CONFIG_URL = FILE_SERVER_ROOT_URL + "app.json";

        ChromeDriver driver;
        HttpFileServer fileServer;

        Runtime runtime;

        [SetUp]
        public void SetUp()
        {
            string dir = Path.GetDirectoryName(GetType().Assembly.Location);
            string dirToServe = Path.Combine(dir, "../../../../src");
            // Serve OpenFin app assets
            fileServer = new HttpFileServer(dirToServe, FILE_SERVER_PORT);
            RuntimeOptions appOptions = RuntimeOptions.LoadManifest(new Uri(APP_CONFIG_URL));
            OPENFIN_APP_RUNTIME = appOptions.Version;
        }

        public void StartOpenfinApp()
        {
            string dir = Path.GetDirectoryName(GetType().Assembly.Location);

            var service = ChromeDriverService.CreateDefaultService();
            service.LogPath = Path.Combine(dir, "chromedriver.log");
            service.EnableVerboseLogging = true;

            var options = new ChromeOptions();

            string runOpenfinPath = Path.Combine(dir, "RunOpenFin.bat");
            string appConfigArg = String.Format("--config={0}", APP_CONFIG_URL);
            if (shareRuntime && runtime != null)
            {
                options.DebuggerAddress = "localhost:4444";
                Process.Start(runOpenfinPath, appConfigArg);
            } else
            {
                options.BinaryLocation = runOpenfinPath;
                options.AddArgument(appConfigArg);
                options.AddArgument(String.Format("--remote-debugging-port={0}", REMOTE_DEBUGGING_PORT));
            }
            driver = new ChromeDriver(service, options);
        }

        private Task<Runtime> ConnectToRuntime()
        {
            var taskCompletionSource = new TaskCompletionSource<Runtime>();

            RuntimeOptions options = new RuntimeOptions();
            options.Version = OPENFIN_ADAPTER_RUNTIME;
            options.Arguments = shareRuntime ? String.Format("--remote-debugging-port={0}", REMOTE_DEBUGGING_PORT) : "";
            runtime = Openfin.Desktop.Runtime.GetRuntimeInstance(options);
            runtime.Connect(() =>
            {
                taskCompletionSource.SetResult(runtime);
            });

            return taskCompletionSource.Task;
        }

        private Task<Runtime> DisconnectFromRuntime()
        {
            var taskCompletionSource = new TaskCompletionSource<Runtime>();

            if (runtime != null && runtime.IsConnected)
            {
                runtime.Disconnect(() =>
                {
                    taskCompletionSource.SetResult(runtime);
                });
            }
            else
            {
                taskCompletionSource.SetResult(runtime);
            }

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
                taskCompletionSource.SetException(new Exception(ack.getJsonObject().ToString()));
            });
            return taskCompletionSource.Task;
        }

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

        private Dictionary<string, object> getProcessInfo()
        {
            string script = "return await fin.System.getProcessList()";
            driver.ExecuteScript(script); // First call is different to following calls
            dynamic processList = driver.ExecuteScript(script);
            return processList[0] as Dictionary<string, object>;
        }

        [Test]
        public async Task GetProcessList()
        {
            StartOpenfinApp();

            var processInfo = getProcessInfo();
            long origWorkingSetSize = (long)processInfo["workingSetSize"];

            Assert.Greater(origWorkingSetSize, 10000000, "working set at least 10MB");

            driver.ExecuteScript("window.location = 'http://www.google.co.uk'");
            await Task.Delay(2000);

            processInfo = getProcessInfo();
            long workingSetSize = (long)processInfo["workingSetSize"];

            Assert.Greater(workingSetSize, 10000000, "working set at least 10MB");

            string returnLocationScript = String.Format("window.location = '{0}index.html'", FILE_SERVER_ROOT_URL);
            driver.ExecuteScript(returnLocationScript);
            await Task.Delay(2000);

            processInfo = getProcessInfo();
            workingSetSize = (long)processInfo["workingSetSize"];

            Assert.Greater(workingSetSize, 10000000, "working set at least 10MB");
            Assert.Greater(workingSetSize, origWorkingSetSize * 0.7, "Similar size to original working set");
            Assert.Less(workingSetSize, origWorkingSetSize * 1.3, "Similar size to original working set");
        }

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

        [TearDown]
        public async Task TearDown()
        {
            fileServer?.Stop();

            StopOpenfinApp();

            await DisconnectFromRuntime();
        }
    }
}
