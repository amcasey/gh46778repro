using ClassLibrary1;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;

namespace ConsoleApp1
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            (var count, var corruptData) = ParseArgs(args);
            new Program().SendCorruptedRequest(count, corruptData);
        }

        public void SendCorruptedRequest(int testCount, bool corruptData)
        {
            for(int i=0; i<testCount; i++)
            {
                using (var client = new TcpClient("127.0.0.1", 7000))
                {
                    // Create SSL stream object
                    var sslStream = new SslStream(new MyCorruptingStreamWrapper(client.GetStream(), corruptData ? 1000 : null));

                    // Create SSL connection with server
                    try
                    {
                        sslStream.AuthenticateAsClient("localhost");
                    }
                    catch (AuthenticationException e)
                    {
                        Tracer.Error($"Exception: {e.Message}");
                        Tracer.Error($"Inner Exception: {e.InnerException?.Message}");
                        return;
                    }

                    // Write HTTP request
                    WriteMessage(sslStream, ResourceFiles.Resources.LocalHttpRequest);

                    // Read HTTP response
                    string serverMessage = ReadMessage(sslStream);

                    // Analyze HTTP response
                    AnalyzeResponse(serverMessage);

                    // Dump service response
                    Tracer.Info("");
                    Tracer.Info($"******************************************************************************************************************");
                    Tracer.Info($"Server response");
                    Tracer.Info($"******************************************************************************************************************");
                    Tracer.Info($"{serverMessage}");
                    Tracer.Info($"******************************************************************************************************************");
                }
            }

            // Dump statistics for all calls
            Tracer.Info("");
            Tracer.Info($"******************************************************************************************************************");
            Tracer.Info($"Final report");
            Tracer.Info($"******************************************************************************************************************");
            Tracer.Info($"Total tests    : {testCount}");
            Tracer.Info($"500 response   : {count500}");
            Tracer.Info($"empty response : {countEmpty}");
            Tracer.Info($"other response : {countOther}");
            Tracer.Info($"******************************************************************************************************************");
        }

        private void AnalyzeResponse(string serverMessage)
        {
            if (serverMessage.Contains("500 Internal Server Error"))
            {
                count500++;
            }
            else if (serverMessage.Trim().Length == 0)
            {
                countEmpty++;
            }
            else
            {
                countOther++;
            }
        }

        private void WriteMessage(SslStream sslStream, string message)
        {
            //sslStream.Write(Encoding.UTF8.GetBytes(message));
             message.Split('\n').ToList().ForEach(line => {
                 var trimmedLine = line.Trim('\r', '\n');
                 Tracer.Verbose($"Sending ({trimmedLine.Length}): {trimmedLine}");
                 sslStream.Write(Encoding.UTF8.GetBytes($"{trimmedLine}\r\n"));
                 Thread.Sleep(1);
             });
            sslStream.Flush();

        }

        private string ReadMessage(SslStream sslStream)
        {
            byte[] buffer = new byte[1024 * 1024];
            StringBuilder messageData = new StringBuilder();
            Decoder decoder = Encoding.UTF8.GetDecoder();

            try
            {
                int bytes = sslStream.Read(buffer, 0, buffer.Length);
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
            }
            catch (IOException x)
            {
                Tracer.Verbose($"Read exception {x.Message}");
                if (x.InnerException is SocketException se && se.SocketErrorCode == SocketError.TimedOut)
                {
                    Tracer.Verbose($"Ignoring SocketException: {se.Message}");
                }
                else
                {
                    throw;
                }
            }

            return messageData.ToString();
        }

        private static (int, bool) ParseArgs(string[] args)
        {
            var count = 1;
            bool corruptData = true;

            if (args != null)
            {
                if (args.Length > 0)
                {
                    count = int.Parse(args[0]);
                }

                if (args.Length > 1)
                {
                    corruptData = bool.Parse(args[1]);
                }
            }

            return (count, corruptData);
        }

        /// <summary>
        /// Sends well formed request to server (useful for Fiddler capture of stream contennts)
        /// </summary>
        private void SendUnCorruptedRequest()
        {
            HttpClient httpClient = new HttpClient(new TracingHandler(new HttpClientHandler() { ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true }));
            var request = new HttpRequestMessage(HttpMethod.Post, "https://127.0.0.1:7000/echo");
            request.Content = new StringContent(JsonSerializer.Serialize(_echoPayload), Encoding.UTF8, "application/json");

            var response = httpClient.Send(request, HttpCompletionOption.ResponseContentRead);
            Tracer.Info($"Response: {response.StatusCode}");

            var content = response.Content.ReadAsStringAsync().Result;
            Tracer.Info($"Content: {content}");
        }

        private static EchoPayload _echoPayload = new EchoPayload
        {
            Message = "Hello from ConsoleApp1",
            Details = new string('x', 1024 * 1)
        };

        private int count500 = 0;
        private int countEmpty = 0;
        private int countOther = 0;

    }
}