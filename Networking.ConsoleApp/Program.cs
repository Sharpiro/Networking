using System;
using System.Configuration;
using System.Threading.Tasks;
using static System.Console;

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

                var server = new Server(ipAddress, port);

                server.MessageReceived += (clientId, message) => { WriteLine($"{clientId}: {message}"); };
                server.ClientAccepted += clientId => { WriteLine($"client '{clientId}' connected"); };
                server.ClientDisconnected += clientId => { WriteLine($"client '{clientId}' disconnected"); };
                server.Started += (ipAddressX, portX) => { WriteLine($"server started on '{ipAddressX}:{portX}'"); };
                server.Stopped += () => { WriteLine("server stopped"); };
                server.DiagnosticsStarted += () => { WriteLine("diagnostics started"); };
                server.DiagnosticsStopped += () => { WriteLine("diagnostics stopped"); };
                server.DiagnosticRun += WriteLine;


                //var _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(t => server.StartDiagnostics(60));
                await server.ListenAsync();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
            finally
            {
                WriteLine("done....");
                ReadKey();
            }
        }
    }
}