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
        HttpFileServer fileServer;

        Runtime runtime;

        [SetUp]
        public void SetUp()
        {
            string dir = Path.GetDirectoryName(GetType().Assembly.Location);
            string dirToServe = Path.Combine(dir, "../../../../src");
            fileServer = new HttpFileServer(dirToServe, 9070);
        }

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
            });
            return taskCompletionSource.Task;
        }

        /* Would prefer to use for inspecting and resizing windows but driver currently raises an exception
         * e.g.
         *         public void ResizeUsingChromeDriver()
        {
            StartOpenfinApp();
            IWindow window = driver.Manage().Window;
            Point initialPos = window.Position; // Crashes here - Browser window not found
            Point newPos = new Point(initialPos.X + 75, initialPos.Y + 180);
            window.Position = newPos;
        }
         * Use OpenFin Javasript API instead
        */
        private Dictionary<string, object> getWindowBounds()
        {
            var script = $@"
const app = fin.Application.getCurrentSync();
// const win = await app.getWindow(); Not actually the app window.  Probably the hidden Provider window
const childWindows = await app.getChildWindows()
const childWin = childWindows[0];
const bounds = await childWin.getBounds();
return bounds;
";
            return driver.ExecuteScript(script) as Dictionary<string, object>;
        }

        private void setWindowBounds(int left, int top, int width, int height)
        {
            var script = $@"
const app = fin.Application.getCurrentSync();
// const win = await app.getWindow(); Not actually the app window.  Probably the hidden Provider window
const childWindows = await app.getChildWindows()
const childWin = childWindows[0];
const bounds = {{
    height: {height},
    width: {width},
    top: {top},
    left: {left}
}}
    await childWin.setBounds(bounds);
";
            driver.ExecuteScript(script);
    }

        [Test]
        public void Test()
        {
            StartOpenfinApp();
            driver.ExecuteScript("alert('My first test')");
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

        [Test]
        public void AppHasDefaultSize()
        {
            StartOpenfinApp();

            var bounds = getWindowBounds();
            Assert.AreEqual(600, bounds["width"]);
            Assert.AreEqual(600, bounds["height"]);
        }

        [Test]
        public void ResizeWindow()
        {
            StartOpenfinApp();
            setWindowBounds(100, 150, 200, 300);
            var bounds = getWindowBounds();
            Assert.AreEqual(100, bounds["left"]);
            Assert.AreEqual(150, bounds["top"]);
            Assert.AreEqual(200, bounds["width"]);
            Assert.AreEqual(300, bounds["height"]);

        }

        [Test]
        public void RestoreSnapshot()
        {
            StartOpenfinApp();
            int newLeft = 100;
            int newTop = 150;
            int newWidth = 200;
            int newHeight = 300;
            setWindowBounds(newLeft, newTop, newWidth, newHeight);

            string createSnapshotScript = $@"
    const platform = await fin.Platform.getCurrent();
    const snapshot = await platform.getSnapshot(); // Raises 'no action registered' exception
    return JSON.stringify(snapshot)
";
            //var snapshot = driver.ExecuteScript(createSnapshotScript);
            string snapshot = @"{""snapshotDetails"":{""timestamp"":""2020-05-21T15:43:37.533Z"",""runtimeVersion"":""15.80.49.30"",""monitorInfo"":{""deviceScaleFactor"":2,""dpi"":{""x"":192,""y"":192},""nonPrimaryMonitors"":[],""primaryMonitor"":{""available"":{""dipRect"":{""top"":23,""bottom"":849,""left"":0,""right"":1440},""scaledRect"":{""top"":46,""bottom"":1698,""left"":0,""right"":2880}},""availableRect"":{""top"":23,""bottom"":849,""left"":0,""right"":1440},""deviceId"":""\\\\?\\DISPLAY#PRL5000#5&140b9d70&0&UID0#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7}"",""deviceScaleFactor"":2,""displayDeviceActive"":true,""dpi"":{""x"":192,""y"":192},""monitor"":{""dipRect"":{""top"":0,""bottom"":900,""left"":0,""right"":1440},""scaledRect"":{""top"":0,""bottom"":1800,""left"":0,""right"":2880}},""monitorRect"":{""top"":0,""bottom"":900,""left"":0,""right"":1440},""name"":""\\\\.\\DISPLAY1""},""reason"":""api-query"",""taskbar"":{""dipRect"":{""top"":860,""bottom"":900,""left"":0,""right"":1440},""edge"":""bottom"",""rect"":{""top"":860,""bottom"":900,""left"":0,""right"":1440},""scaledRect"":{""top"":1720,""bottom"":1800,""left"":0,""right"":2880}},""virtualScreen"":{""top"":0,""bottom"":900,""left"":0,""right"":1440,""dipRect"":{""top"":0,""bottom"":900,""left"":0,""right"":1440},""scaledRect"":{""top"":0,""bottom"":1800,""left"":0,""right"":2880}}}},""windows"":[{""autoShow"":true,""contextMenuSettings"":{""enable"":true,""devtools"":true,""reload"":false},""defaultCentered"":false,""defaultHeight"":300,""defaultLeft"":100,""defaultTop"":150,""defaultWidth"":200,""maxHeight"":-1,""maxWidth"":-1,""maximizable"":true,""minHeight"":200,""minWidth"":200,""minimizable"":true,""name"":""OpenFin appseed"",""resizable"":true,""resizeRegion"":{""bottomRightCorner"":20,""size"":7,""sides"":{""top"":true,""right"":true,""bottom"":true,""left"":true}},""saveWindowState"":false,""state"":""normal"",""url"":""http://localhost:9070/index.html"",""title"":""OpenFin appseed"",""height"":300,""width"":200,""x"":100,""y"":150}]}";
            StopOpenfinApp();
            StartOpenfinApp();

            // Reloads with default size
            var bounds = getWindowBounds();
            Assert.AreEqual(600, bounds["width"]);
            Assert.AreEqual(600, bounds["height"]);

            string escapedSnapshot = snapshot.Replace(@"\", @"\\"); // "\"s will be unescaped when injected into script string

            string restoreSnapshotScript = $@"
const platform = await fin.Platform.getCurrent();
const snapshot = JSON.parse('{escapedSnapshot}');  // Raises 'no action registered' exception
await platform.applySnapshot(snapshot);
";
            driver.ExecuteScript(restoreSnapshotScript);

            bounds = getWindowBounds();
            Assert.AreEqual(newLeft, bounds["left"]);
            Assert.AreEqual(newTop, bounds["top"]);
            Assert.AreEqual(newWidth, bounds["width"]);
            Assert.AreEqual(newHeight, bounds["height"]);
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
            if (fileServer != null)
            {
                fileServer.Stop();
            }
            StopOpenfinApp();

            await DisconnectFromRuntime();
        }
    }
}
