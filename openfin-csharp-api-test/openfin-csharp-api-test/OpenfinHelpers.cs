using Openfin.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenfinDesktop
{
    class OpenfinHelpers
    {
        public static Task<Runtime> ConnectToRuntime(string version, string arguments = "")
        {
            var taskCompletionSource = new TaskCompletionSource<Runtime>();

            RuntimeOptions options = new RuntimeOptions();
            options.Version = version;
            options.Arguments = arguments;
            Runtime runtime = Runtime.GetRuntimeInstance(options);
            runtime.Connect(() =>
            {
                taskCompletionSource.SetResult(runtime);
            });

            return taskCompletionSource.Task;
        }

        public static Task DisconnectFromRuntime(Runtime runtime)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            if (runtime != null && runtime.IsConnected)
            {
                runtime.Disconnect(() =>
                {
                    taskCompletionSource.SetResult(true);
                });
            }
            else
            {
                taskCompletionSource.SetResult(true);
            }

            return taskCompletionSource.Task;
        }
    }
}
