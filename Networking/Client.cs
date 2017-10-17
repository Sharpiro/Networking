using Networking.Models;
using Networking.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
    public class TheClient : BaseSocket
    {
        private TcpClient _client;
        private CancellationTokenSource _receiveCts;

        public TcpClient TcpClient => _client;
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

        public TheClient(TcpClient tcpClient)
        {
            _client = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            IpAddress = RemoteEndPoint.Address.ToString();
            Port = RemoteEndPoint.Port;
        }

        public async Task ConnectAsync()
        {
            if (_client == null)
            {
                _client = new TcpClient();
            }
            await _client.ConnectAsync(IPAddress.Parse(IpAddress), Port);
            await HandShake();
            OnConnected(IpAddress, Port);
            _receiveCts = new CancellationTokenSource();
            var _ = ReceiveMessagesAsync();
        }

        public async Task HandShake()
        {
            Id = Guid.NewGuid().ToString();
            var socketMessage = new SocketMessage
            {
                ClientId = Id,
                Title = nameof(HandShake),
                //Data = Encoding.UTF8.GetBytes("Handshake"),
                MessageType = MessageType.Command,
                SentUtc = DateTime.UtcNow
            };
            await SendMessageAsync(socketMessage);
        }

        public async Task DisconnectAsync()
        {
            //if (!IsConnected) return;
            await Task.Yield();
            _client.Close();
            _client = null;
            OnDisconnected(IpAddress, Port);
        }

        public Task SendMessageAsync(string message)
        {
            var messageDto = new SocketMessage
            {
                ClientId = Id,
                Data = Encoding.UTF8.GetBytes(message),
                MessageType = MessageType.PlainText,
                SentUtc = DateTime.UtcNow
            };
            return SendMessageAsync(messageDto);
        }

        public async Task SendMessageAsync(SocketMessage message)
        {
            if (!IsConnected) throw new InvalidOperationException("Cannot send message, client is not conencted");
            if (message == null) throw new ArgumentNullException(nameof(message));

            var stream = _client.GetStream();
            var messageJson = JsonConvert.SerializeObject(message);

            var buffer = Encoding.UTF8.GetBytes(messageJson);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task ReceiveMessagesAsync()
        {
            var stream = _client.GetStream();
            var buffer = new byte[1024];
            while (!_receiveCts.IsCancellationRequested)
            {
                var bufferSize = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bufferSize == 0) await DisconnectAsync();
                var data = new byte[bufferSize];
                Array.Copy(buffer, data, bufferSize);
                buffer.Clear();
                var jsonData = Encoding.UTF8.GetString(data);
                var message = JsonConvert.DeserializeObject<SocketMessage>(jsonData);
                message.ReceivedUtc = DateTime.UtcNow;
                if (message.MessageType == MessageType.Command) HandleCommand(message);
                else OnMessageReceived(message);
            }
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