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

        public bool IsConnected => _client?.Connected ?? false;

        public event Action Connected;
        public event Action Disconnected;
        public event Action ConnectionChanged;
        public event Action<string> MessageReceived;

        public TheClient(string ipAddress, int port)
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            Port = port > 0 && port < 65536 ? port : throw new ArgumentOutOfRangeException(nameof(port));
        }

        public async Task Connect()
        {
            if (_client == null)
            {
                _client = new TcpClient();
            }
            await _client.ConnectAsync(IPAddress.Parse(IpAddress), Port);
            OnConnected();
            var _ = ReceiveMessagesAsync();
        }

        public async Task Disconnect()
        {
            //if (!IsConnected) return;
            await Task.Yield();
            _client.Close();
            _client = null;
            OnDisconnected();
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
            var message = Encoding.UTF8.GetString(data);
            OnMessageReceived(message);
        }

        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke();
            OnConnectionChanged();
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke();
            OnConnectionChanged();
        }

        protected virtual void OnConnectionChanged() => ConnectionChanged?.Invoke();
        protected virtual void OnMessageReceived(string message) => MessageReceived.Invoke(message);
    }
}