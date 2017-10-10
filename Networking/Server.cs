using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Networking.Tools;
using static System.Console;
using Networking.Models;

namespace Networking
{
    public class Server : BaseSocket
    {
        private TcpListener _tcpListener;
        private readonly Dictionary<string, SocketInfo> _clients = new Dictionary<string, SocketInfo>(StringComparer.InvariantCultureIgnoreCase);
        private CancellationTokenSource _diagnosticsCts;
        private CancellationTokenSource _listenCts;

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool DiagnosticsRunning => !_diagnosticsCts?.IsCancellationRequested ?? false;
        public bool IsListening { get; private set; }

        public event Action<string> ClientAccepted;
        public event Action<string> ClientDisconnected;
        public event Action<string, SocketMessage> MessageReceived;
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
            while (!_listenCts.Token.IsCancellationRequested)
            {
                var newCLient = await _tcpListener.AcceptTcpClientAsync();
                var clientId = Guid.NewGuid().ToString();
                _clients.Add(clientId, new SocketInfo(clientId, newCLient.Client));
                OnClientAccepted(clientId);
                var _ = ListenToClientAsync(clientId, newCLient);
            }
        }

        public async Task StopAsync()
        {
            if (_tcpListener == null) return;
            //if (_tcpListener == null) throw new InvalidOperationException($"The server is not curently running.");
            await Task.Yield();
            foreach (var clientData in _clients)
            {
                clientData.Value.TcpClient.Close();
            }
            _clients.Clear();
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

        private async Task ListenToClientAsync(string clientId, TcpClient client)
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];
            while (!_listenCts.Token.IsCancellationRequested)
            {
                var bufferSize = await stream.ReadAsync(buffer, 0, buffer.Length, _listenCts.Token);
                if (bufferSize == 0)
                {
                    client.Close();
                    OnClientDisconnected(clientId);
                    break;
                }
                var data = new byte[bufferSize];
                Array.Copy(buffer, data, bufferSize);
                buffer.Clear();
                var stringData = Encoding.UTF8.GetString(data);
                var message = new SocketMessage
                {
                    MessageType = MessageType.PlainText,
                    Data = data,
                    ReceivedUtc = DateTime.UtcNow
                };
                if (message.MessageType == MessageType.Command) HandleCommand(message);
                else OnMessageReceived(clientId, message);
            }
            WriteLine("stopped listening on client xyz...");
        }

        private async Task PrintDiagnosticsAsync(CancellationToken cancellationToken, int intervalSeconds)
        {
            var builder = new StringBuilder();
            while (!cancellationToken.IsCancellationRequested)
            {
                builder.AppendLine($"Diagnostics: {DateTime.UtcNow}----------------------------");
                foreach (var client in _clients)
                {
                    builder.AppendLine($"'{client.Key}':\t'{client.Value.Connected}'\t'{client.Value.LocalEndPoint}'\t'{client.Value.RemoteEndPoint}'");
                }
                OnDiagnosticRun(builder.ToString());
                builder.Clear();
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);
            }
        }

        protected virtual void OnClientAccepted(string clientId) => ClientAccepted?.Invoke(clientId);
        protected virtual void OnClientDisconnected(string clientId) => ClientDisconnected?.Invoke(clientId);
        protected virtual void OnMessageReceived(string clientId, SocketMessage message) => MessageReceived?.Invoke(clientId, message);
        protected virtual void OnStarted(string ip, int port) => Started?.Invoke(ip, port);
        protected virtual void OnStopped() => Stopped?.Invoke();
        protected virtual void OnStartedDiagnostics() => DiagnosticsStarted?.Invoke();
        protected virtual void OnStoppedDiagnostics() => DiagnosticsStopped?.Invoke();
        protected virtual void OnDiagnosticRun(string diagnostics) => DiagnosticRun?.Invoke(diagnostics);
    }
}