using System;
using System.Threading;
using System.Threading.Tasks;

namespace Networking.ConsoleApp
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                const string ipAddress = "127.0.0.1";
                const int port = 500;
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