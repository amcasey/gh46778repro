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
        private static EchoPayload _echoPayload = new EchoPayload
        {
            Message = "Hello from ConsoleApp1",
            Details = new string('x', 1024 * 1)
        };

        private int count500 = 0;
        private int countEmpty = 0;
        private int countOther = 0;
        
        public static void Main(string[] args)
        {
            var count = 1;
            bool corruptData = true;

            if (args != null && args.Length > 0 && int.TryParse(args[0], out var parsedCount))
            {
                count = parsedCount;
            }

            if (args != null && args.Length > 1 && bool.TryParse(args[1], out var parsedCorruptData))
            {
                corruptData = parsedCorruptData; ;
            }

            //new Program().SendUnCorruptedRequest();
            new Program().SendCorruptedRequest(count, corruptData);
        }

        public void SendCorruptedRequest(int testCount, bool corruptData)
        {
            for(int i=0; i<testCount; i++)
            {
                using (var client = new TcpClient("127.0.0.1", 7000))
                {
                    // Create SSL stream object
                    var sslStream = new SslStream(new MyCorruptingStreamWrapper(client.GetStream(), corruptData ? 1000 : null), false, (s, c, cc, ss) => true, null) { ReadTimeout = 50 };

                    // Create SSL connection with server
                    try
                    {
                        sslStream.AuthenticateAsClient("127.0.0.1");
                    }
                    catch (AuthenticationException e)
                    {
                        Tracer.Error($"Exception: {e.Message}");
                        Tracer.Error($"Inner Exception: {e.InnerException?.Message}");
                        return;
                    }

                    // Send request to server over SSL connection
                    ResourceFiles.Resources.LocalHttpRequest.Split('\n').ToList().ForEach(line =>
                    {
                        var trimmedLine = line.Trim('\r', '\n');
                        Tracer.Verbose($"Sending ({trimmedLine.Length}): {trimmedLine}");
                        sslStream.Write(Encoding.UTF8.GetBytes($"{trimmedLine}\r\n"));
                        Thread.Sleep(1);
                    });
                    sslStream.Flush();

                    // Read response from server
                    string serverMessage = ReadMessage(sslStream);
                    AnalyzeResponse(serverMessage);
                    Tracer.Info("");
                    Tracer.Info($"******************************************************************************************************************");
                    Tracer.Info($"Server response");
                    Tracer.Info($"******************************************************************************************************************");
                    Tracer.Info($"{serverMessage}");
                    Tracer.Info($"******************************************************************************************************************");
                }
            }

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

        private string ReadMessage(SslStream sslStream)
        {
            byte[] buffer = new byte[1024 * 1024];
            StringBuilder messageData = new StringBuilder();
            Decoder decoder = Encoding.UTF8.GetDecoder();

            try
            {
                int bytes = sslStream.Read(buffer, 0, buffer.Length);
                while (bytes > 0)
                {
                    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                    decoder.GetChars(buffer, 0, bytes, chars, 0);
                    messageData.Append(chars);

                    bytes = sslStream.Read(buffer, 0, buffer.Length);
                };
            }
            catch (IOException x)
            {
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
    }
}