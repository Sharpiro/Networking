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
        private readonly string _ipAddress;
        private readonly int _port;

        public bool IsConnected => _client?.Connected ?? false;
        public event Action Connected;
        public event Action Disconnected;
        public event Action ConnectionChanged;

        public TheClient(string ipAddress, int port)
        {
            _port = port > 0 && port < 65536 ? port : throw new ArgumentOutOfRangeException(nameof(port));
            _ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            _client = new TcpClient();
        }

        public async Task Connect()
        {
            if (_client == null)
            {
                _client = new TcpClient();
            }
            await _client.ConnectAsync(IPAddress.Parse(_ipAddress), _port);
            OnConnected();
        }

        public async Task Disconnect()
        {
            if (!IsConnected) return;
            await Task.Yield();
            _client.Close();
            _client = null;
            OnDisconnected();
        }

        public async Task SendMessage(string message)
        {
            if (!IsConnected) throw new InvalidOperationException("Cannot send message, client is not conencted");
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
            var stream = _client.GetStream();
            var buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
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
    }
}