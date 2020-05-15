using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenfinDesktop
{
    class HttpFileServer
    {
        private HttpListenerResponse response;
        private HttpListener listener;
        private string baseFilesystemPath;

        public HttpFileServer(String dirToServe, int port)
        {
            baseFilesystemPath = Path.GetFullPath(dirToServe);

            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();

            Console.WriteLine("--- Server stated, base path is: " + baseFilesystemPath);

            try
            {
                ServerLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (response != null)
                {
                    SendErrorResponse(500, "Internal server error");
                }
            }
        }

        public void Stop()
        {
            listener.Stop();
        }
            
        private async void ServerLoop()
        {
            while (listener.IsListening)
            {
                HttpListenerContext context; 
                try
                {
                    context = await listener.GetContextAsync();
                }
                catch (Exception e)
                {
                    if (listener.IsListening == false)
                    {
                        break;
                    }
                    else
                    {
                        throw e;
                    }
                }

                var request = context.Request;
                response = context.Response;
                var fileName = request.RawUrl.Substring(1);
                Console.WriteLine(
                    "--- Got {0} request for: {1}",
                    request.HttpMethod, fileName);

                if (request.HttpMethod.ToUpper() != "GET")
                {
                    SendErrorResponse(405, "Method must be GET");
                    continue;
                }

                var fullFilePath = Path.Combine(baseFilesystemPath, fileName);
                if (!File.Exists(fullFilePath))
                {
                    SendErrorResponse(404, "File not found");
                    continue;
                }

                Console.Write("    Sending file...");
                using (var fileStream = File.OpenRead(fullFilePath))
                {
                    string ext = Path.GetExtension(fileName);
                    if (ext == ".html")
                    {
                        response.ContentType = "text/html";
                    }
                    else if (ext == ".json")
                    {
                        response.ContentType = "application/json";
                    }
                    else
                    {
                        response.ContentType = "text/plain";
                    }
                    response.ContentLength64 = (new FileInfo(fullFilePath)).Length;
                    fileStream.CopyTo(response.OutputStream);
                }

                response.OutputStream.Close();
                response = null;
                Console.WriteLine(" Ok!");
            }
        }

        void SendErrorResponse(int statusCode, string statusResponse)
        {
            response.ContentLength64 = 0;
            response.StatusCode = statusCode;
            response.StatusDescription = statusResponse;
            response.OutputStream.Close();
            Console.WriteLine("*** Sent error: {0} {1}", statusCode, statusResponse);
        }
    }
}
