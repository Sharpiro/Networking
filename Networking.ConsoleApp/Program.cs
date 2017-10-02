using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Networking.ConsoleApp
{
    public class Program
    {
        private static async Task Main()
        {
            try
            {
                var ipAddress = ConfigurationManager.AppSettings["ipaddress"];
                var port = int.Parse(ConfigurationManager.AppSettings["port"]);
                var tokenSource = new CancellationTokenSource();

                var server = new Server(ipAddress, port);
                await server.Listen(tokenSource.Token);
                Console.WriteLine("done....");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}