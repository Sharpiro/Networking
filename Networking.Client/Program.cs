using System;
using System.Threading.Tasks;

namespace Networking.Client
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                const string ipAddress = "127.0.0.1";
                const int port = 500;

                var client = new TheClient(ipAddress, port);
                await client.Connect();
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
