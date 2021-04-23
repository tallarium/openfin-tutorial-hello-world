using Openfin.Desktop;
using System.Threading.Tasks;

namespace OpenfinDesktop
{
    class Program
    {
        static async Task EnsureRuntimes()
        {
            OpenfinTests openfinTests = new OpenfinTests();
            //openfinTests.SetUp();
            //Runtime runtime = await OpenfinHelpers.ConnectToRuntime(openfinTests.OPENFIN_APP_RUNTIME);
            //await OpenfinHelpers.DisconnectFromRuntime(runtime);
            //if (OpenfinTests.OPENFIN_ADAPTER_RUNTIME != openfinTests.OPENFIN_APP_RUNTIME)
            //{
            await OpenfinHelpers.ConnectToRuntime(OpenfinTests.OPENFIN_ADAPTER_RUNTIME);
            //}
        }

        static void Main(string[] args)
        {
            var task = Task.Run(async () => { await EnsureRuntimes(); });
            task.Wait();
        }
    }
}
