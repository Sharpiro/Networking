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
                var socket1 = new TheClient(ipAddress, port);
                socket1.Connected += (ip, portX) => Console.WriteLine($"connected to '{ip}:{portX}'");
                socket1.Disconnected += (ip, portX) => Console.WriteLine($"disconnected from '{ip}:{portX}'");

                //connect to remote
                await socket1.ConnectAsync();

                //listen on local endpoint used to connect to the previous remote
                var socketOnelocalEndPoint = socket1.LocalEndPoint;
                var socketOnelocalIp = socketOnelocalEndPoint.Address.ToString();
                var socketOnelocalPort = socketOnelocalEndPoint.Port;
                var socket2 = new Server(socketOnelocalIp, socketOnelocalPort);
                socket2.Started += (ip, portX) => Console.WriteLine($"started listening on '{ip}:{portX}'");
                socket2.Stopped += () => Console.WriteLine("stopped listening");
                socket2.ClientAccepted += (clientId) => Console.WriteLine($"accepted client '{clientId}'");
                await socket2.ListenAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("done....");
                Console.ReadKey();
            }
        }
    }
}