using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddHttpLogging(options => {
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
            });

            builder.WebHost.ConfigureKestrel((context, serverOptions) => {
                serverOptions.ListenLocalhost(7000, listenOptions => {
                    //listenOptions.UseConnectionLogging(); // Encrypted
                    // listenOptions.UseHttps("/home/acasey/keys/localhost.p12", "mypass");
                    listenOptions.UseHttps();
                    listenOptions.UseConnectionLogging(); // Decrypted
                });
            });

            var app = builder.Build();

            //
            //  Uncomment the following lines to catch the decryption error and return a 400 response to the client.
            //
            //app.Use(async (context, next) =>
            //{
            //    try
            //    {
            //        await next(context);
            //    }
            //    catch (IOException x)
            //    {
            //        if ((x.InnerException is System.ComponentModel.Win32Exception wx) && (wx.NativeErrorCode == unchecked((int)0x80090330)))
            //        {
            //                app.Logger.LogWarning($"Client encryption error detected!  Returning 400 to caller.");
            //
            //                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            //                context.Response.ContentType = "application/json";
            //                await context.Response.WriteAsync(JsonSerializer.Serialize(new { Code = $"TlsDecryption", Message = $"Could not decrypt client transmitted data" }));
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //});

            app.UseHttpsRedirection();
            app.UseHttpLogging();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}