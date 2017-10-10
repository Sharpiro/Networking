using Networking.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public class TheClient
    {
        private TcpClient _client;
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public IPEndPoint LocalEndPoint => (IPEndPoint)_client.Client.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => (IPEndPoint)_client.Client.RemoteEndPoint;

        public bool IsConnected => _client?.Connected ?? false;

        public event Action<string, int> Connected;
        public event Action<string, int> Disconnected;
        public event Action ConnectionChanged;
        public event Action<SocketMessage> MessageReceived;

        public TheClient(string ipAddress, int port)
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            Port = port > 0 && port < 65536 ? port : throw new ArgumentOutOfRangeException(nameof(port));
        }

        public async Task ConnectAsync()
        {
            if (_client == null)
            {
                _client = new TcpClient();
            }
            await _client.ConnectAsync(IPAddress.Parse(IpAddress), Port);
            OnConnected(IpAddress, Port);
            var _ = ReceiveMessagesAsync();
        }

        public async Task Disconnect()
        {
            //if (!IsConnected) return;
            await Task.Yield();
            _client.Close();
            _client = null;
            OnDisconnected(IpAddress, Port);
        }

        public async Task SendMessage(string message)
        {
            //if (!IsConnected) throw new InvalidOperationException("Cannot send message, client is not conencted");
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
            var stream = _client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task ReceiveMessagesAsync()
        {
            var stream = _client.GetStream();
            var buffer = new byte[1024];
            var bufferSize = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bufferSize == 0) await Disconnect();
            var data = new byte[bufferSize];
            var message = new SocketMessage
            {
                MessageType = MessageType.PlainText,
                Data = data,
                ReceivedUtc = DateTime.UtcNow
            };
            OnMessageReceived(message);
        }

        protected virtual void OnDisconnected(string ipAddress, int port)
        {
            Disconnected?.Invoke(ipAddress, port);
            OnConnectionChanged();
        }

        protected virtual void OnConnected(string ipAddress, int port)
        {
            Connected?.Invoke(ipAddress, port);
            OnConnectionChanged();
        }

        protected virtual void OnConnectionChanged() => ConnectionChanged?.Invoke();
        protected virtual void OnMessageReceived(SocketMessage message) => MessageReceived.Invoke(message);
    }
}