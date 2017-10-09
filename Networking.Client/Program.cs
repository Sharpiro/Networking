using System;
using System.Configuration;
using System.Net;
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

                var socket1 = new TcpSocket();
                await socket1.ConnectAsync(ipAddress, port);
                var localEndPoint = (socket1.MySocket.LocalEndPoint as IPEndPoint);
                var localIp = localEndPoint.Address.ToString();
                //await socket1.ListenAsync(localIp, localEndPoint.Port);
                var socket2 = new TcpSocket();
                var receivedSocket = await socket2.ListenAsync(localIp, localEndPoint.Port);
                //await socket2.ConnectAsync(ipAddress, port);
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
