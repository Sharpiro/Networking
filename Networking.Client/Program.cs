using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Networking.Client
{
    public class Program
    {
        private static async Task Main()
        {
            try
            {

                var ipAddress = ConfigurationManager.AppSettings["ipaddress"];
                var port = int.Parse(ConfigurationManager.AppSettings["port"]);

                var socket = new TcpSocket();
                var receivedSocket = await socket.ListenAsync(ipAddress, port);
                //await client.ConnectAsync(ipAddress, port);
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
