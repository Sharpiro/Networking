using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
    public class Server
    {
        private TcpListener _tcpListener;
        private List<TcpClient> _clients = new List<TcpClient>();

        public Server(string ipAddress, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public async Task Listen(CancellationToken cancellationToken)
        {
            _tcpListener.Start();
            Console.WriteLine("listening...");
            while (!cancellationToken.IsCancellationRequested)
            {
                var newCLient = await _tcpListener.AcceptTcpClientAsync();

                _clients.Add(newCLient);
                var listenTask = ListenToClientAsync(newCLient, cancellationToken);
                Console.WriteLine($"Added client {_clients.Count}...");
            }
            Console.WriteLine("no longer listening...");
        }

        private async Task ListenToClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];
            while (!cancellationToken.IsCancellationRequested)
            {
                var bufferSize = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bufferSize == 0)
                {
                    client.Close();
                    break;
                }
                var data = new byte[bufferSize];
                Array.Copy(buffer, data, bufferSize);
                var stringData = Encoding.UTF8.GetString(data);
                Console.WriteLine(stringData);
            }
            Console.WriteLine("stopped listening on client xyz...");
        }
    }
}