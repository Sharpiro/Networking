using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Networking.Tools;
using Networking.Models;
using System.Linq;
using Newtonsoft.Json;

namespace Networking
{
    public class Server : BaseSocket
    {
        private TcpListener _tcpListener;
        private readonly ClientList _clients = new ClientList();
        private CancellationTokenSource _diagnosticsCts;
        private CancellationTokenSource _listenCts;

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool DiagnosticsRunning => !_diagnosticsCts?.IsCancellationRequested ?? false;
        public bool IsListening { get; private set; }

        public ClientList Clients => _clients;

        public event Action ClientAccepted;
        public event Action<string> ClientHandshake;
        public event Action<string> ClientDisconnected;
        public event Action<SocketMessage> MessageReceived;
        public event Action<string, int> Started;
        public event Action Stopped;
        public event Action DiagnosticsStarted;
        public event Action DiagnosticsStopped;
        public event Action<string> DiagnosticRun;

        public Server(string ipAddress, int port)
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            Port = port > 0 && port < 65536 ? port : throw new ArgumentOutOfRangeException(nameof(port));
        }

        public async Task ListenAsync()
        {
            if (IsListening || _tcpListener != null) throw new InvalidOperationException($"The server is currently running on '{_tcpListener.LocalEndpoint}'.  You must stop the server first.");
            _tcpListener = new TcpListener(IPAddress.Parse(IpAddress), Port);
            _listenCts = new CancellationTokenSource();
            _tcpListener.Start();
            IsListening = true;
            OnStarted(IpAddress, Port);
            while (!_listenCts.IsCancellationRequested)
            {
                TcpClient tcpClient;
                try
                {
                    tcpClient = await _tcpListener.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException ex)
                {
                    if (!_listenCts.IsCancellationRequested) throw new InvalidOperationException($"Unexpected disposure of listening socket @ '{IpAddress}:{Port}'", ex);
                    break;
                }
                //var clientId = Guid.NewGuid().ToString();
                var newClient = new TheClient(tcpClient);
                Clients.Add(null, newClient);
                OnClientAccepted();
                var _ = ListenToClientAsync(newClient);
            }
        }

        public async Task StopAsync()
        {
            if (_tcpListener == null) return;
            //if (_tcpListener == null) throw new InvalidOperationException($"The server is not curently running.");
            await Task.Yield();
            foreach (var clientData in Clients.Where(c => c.IsConnected))
            {
                clientData.TcpClient.Client.Shutdown(SocketShutdown.Both);
                clientData.TcpClient.Client.Close();
            }
            Clients.Clear();
            _listenCts.Cancel();
            _tcpListener.Server.Close();
            _tcpListener.Stop();
            _tcpListener = null;
            StopDiagnostics();
            OnStopped();
            IsListening = false;
        }

        public void StartDiagnostics(int intervalSeconds = 30)
        {
            if (!IsListening) return;
            if (!_diagnosticsCts?.IsCancellationRequested ?? false) return;
            _diagnosticsCts = new CancellationTokenSource();
            var _ = PrintDiagnosticsAsync(_diagnosticsCts.Token, intervalSeconds);
            OnStartedDiagnostics();
        }

        public void StopDiagnostics()
        {
            if (!IsListening) return;
            _diagnosticsCts?.Cancel();
            OnStoppedDiagnostics();
        }

        private async Task ListenToClientAsync(TheClient client)
        {
            var stream = client.TcpClient.GetStream();
            var buffer = new byte[1024];
            while (!_listenCts.Token.IsCancellationRequested)
            {
                var bufferSize = await stream.ReadAsync(buffer, 0, buffer.Length, _listenCts.Token);
                if (bufferSize == 0)
                {
                    client.TcpClient.Close();
                    OnClientDisconnected(client.Id);
                    break;
                }
                var data = new byte[bufferSize];
                Array.Copy(buffer, data, bufferSize);
                buffer.Clear();
                var jsonData = Encoding.UTF8.GetString(data);
                var message = JsonConvert.DeserializeObject<SocketMessage>(jsonData);

                if (client.Id == null) client.Id = message.ClientId;
                //message.ClientId = client.Id;
                //message.Client = client;
                message.ReceivedUtc = DateTime.UtcNow;
                if (message.MessageType == MessageType.Command) HandleCommand(message);
                else OnMessageReceived(message);
            }
        }

        private async Task PrintDiagnosticsAsync(CancellationToken cancellationToken, int intervalSeconds)
        {
            var builder = new StringBuilder();
            while (!cancellationToken.IsCancellationRequested)
            {
                builder.AppendLine($"Diagnostics: {DateTime.UtcNow}----------------------------");
                foreach (var client in Clients)
                {
                    builder.AppendLine($"'{client.Id}':\t'{client.IsConnected}'\t'{client.LocalEndPoint}'\t'{client.RemoteEndPoint}'");
                }
                OnDiagnosticRun(builder.ToString());
                builder.Clear();
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);
            }
        }

        public async Task SendMessageAsync(string clientId, string message)
        {
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(nameof(clientId));
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
            var client = Clients.Get(clientId);
            if (client == null) throw new NullReferenceException($"Could not find client with id '{clientId}'");
            var messageDto = new SocketMessage
            {
                Data = Encoding.UTF8.GetBytes(message),
                SentUtc = DateTime.UtcNow,
                MessageType = MessageType.PlainText
            };
            await SendMessageAsync(clientId, messageDto);
        }

        public async Task SendMessageAsync(string clientId, SocketMessage message)
        {
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(nameof(clientId));
            if (message == null) throw new ArgumentNullException(nameof(message));
            var client = Clients.Get(clientId);
            if (client == null) throw new NullReferenceException($"Could not find client with id '{clientId}'");
            await client.SendMessageAsync(message);
        }

        public async Task BroadcastMessageAsync(SocketMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            foreach (var client in Clients.Where(c => c.IsConnected))
            {
                await client.SendMessageAsync(message);
            }
        }

        private async Task ConnectToPeers(SocketMessage socketMessage)
        {
            var serializableList = Clients.GetSerializableList();
            var json = JsonConvert.SerializeObject(serializableList);
            var responseJson = new SocketMessage
            {
                Title = nameof(ConnectToPeers),
                Data = Encoding.UTF8.GetBytes(json),
                SentUtc = DateTime.UtcNow,
                MessageType = MessageType.Command
            };
            await BroadcastMessageAsync(responseJson);
        }

        private void HandShake(SocketMessage message)
        {
            OnClientHandshake(message.ClientId);
            //await ConnectToPeers(message);
        }

        protected virtual void OnClientAccepted() => ClientAccepted?.Invoke();
        protected virtual void OnClientHandshake(string clientId) => ClientHandshake?.Invoke(clientId);
        protected virtual void OnClientDisconnected(string clientId) => ClientDisconnected?.Invoke(clientId);
        protected virtual void OnMessageReceived(SocketMessage message) => MessageReceived?.Invoke(message);
        protected virtual void OnStarted(string ip, int port) => Started?.Invoke(ip, port);
        protected virtual void OnStopped() => Stopped?.Invoke();
        protected virtual void OnStartedDiagnostics() => DiagnosticsStarted?.Invoke();
        protected virtual void OnStoppedDiagnostics() => DiagnosticsStopped?.Invoke();
        protected virtual void OnDiagnosticRun(string diagnostics) => DiagnosticRun?.Invoke(diagnostics);
    }
}