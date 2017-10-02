using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Networking.Tools;
using static System.Console;

namespace Networking
{
    public class Server
    {
        private readonly TcpListener _tcpListener;
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private Task _diagnosticTask;

        public Server(string ipAddress, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public async Task Listen(CancellationToken cancellationToken)
        {
            _tcpListener.Start();
            _diagnosticTask = PrintDiagnostics(cancellationToken);
            WriteLine($"listening on {_tcpListener.LocalEndpoint}...");
            while (!cancellationToken.IsCancellationRequested)
            {
                var newCLient = await _tcpListener.AcceptTcpClientAsync();

                _clients.Add(newCLient);
                var listenTask = ListenToClientAsync(newCLient, cancellationToken);
                WriteLine($"Added client {_clients.Count}...");
            }
            WriteLine("no longer listening...");
        }

        private async Task ListenToClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];
            while (!cancellationToken.IsCancellationRequested)
            {
                var bufferSize = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                //if (bufferSize == 0)
                //{
                //    client.Close();
                //    break;
                //}
                var data = new byte[bufferSize];
                Array.Copy(buffer, data, bufferSize);
                buffer.Clear();
                var stringData = Encoding.UTF8.GetString(data);
                var byteData = string.Join(string.Empty, data.Select(b => b.ToString("X2")));
                WriteLine($"{byteData} - {stringData} - {DateTime.UtcNow}");
            }
            WriteLine("stopped listening on client xyz...");
        }

        private async Task PrintDiagnostics(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                WriteLine($"Diagnostics: {DateTime.UtcNow}----------------------------");
                for (var i = 0; i < _clients.Count; i++)
                {
                    WriteLine($"client '{i}': {_clients[i].Connected}\t{_clients[i].Client.Connected}\t{_clients[i].Client.Ttl}");
                }
            }
        }
    }
}