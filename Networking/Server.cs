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
        public int Port { get; set; }
        private readonly TcpListener _tcpListener;
        private readonly Dictionary<string, TcpClient> _clients = new Dictionary<string, TcpClient>(StringComparer.InvariantCultureIgnoreCase);
        private CancellationTokenSource _diagnosticsCts;

        public string IpAddress { get; set; }
        public bool DiagnosticsRunning => !_diagnosticsCts?.IsCancellationRequested ?? false;
        public bool IsListening { get; private set; }

        public event Action<string> ClientConnected;
        public event Action<string, string> MessageReceived;
        public event Action<string> Started;
        public event Action Stopped;
        public event Action DiagnosticsStarted;
        public event Action DiagnosticsStopped;
        public event Action<string> DiagnosticRun;

        public Server(string ipAddress, int port)
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            Port = port > 0 && port < 65536 ? port : throw new ArgumentOutOfRangeException(nameof(port));
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public async Task Listen(CancellationToken cancellationToken)
        {
            _tcpListener.Start();
            IsListening = true;
            OnStarted(_tcpListener.LocalEndpoint.ToString());
            WriteLine($"listening on {_tcpListener.LocalEndpoint}...");
            while (!cancellationToken.IsCancellationRequested)
            {
                var newCLient = await _tcpListener.AcceptTcpClientAsync();
                var clientId = Guid.NewGuid().ToString();
                _clients.Add(clientId, newCLient);
                OnClientConnected(clientId);
                var _ = ListenToClientAsync(clientId, newCLient, cancellationToken);
                WriteLine($"Added client {_clients.Count}...");
            }
            WriteLine("no longer listening...");
        }

        public async Task Stop()
        {
            await Task.Yield();
            _tcpListener.Stop();
            IsListening = false;
            OnStopped();
        }

        public void StartDiagnostics()
        {
            if (!IsListening) return;
            if (!_diagnosticsCts?.IsCancellationRequested ?? false) return;
            _diagnosticsCts = new CancellationTokenSource();
            var _ = PrintDiagnostics(_diagnosticsCts.Token);
            OnStartedDiagnostics();
        }

        public void StopDiagnostics()
        {
            if (!IsListening) return;
            _diagnosticsCts?.Cancel();
            OnStoppedDiagnostics();
        }

        private async Task ListenToClientAsync(string clientId, TcpClient client, CancellationToken cancellationToken)
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
                var message = $"{byteData} - {stringData} - {DateTime.UtcNow}";
                WriteLine($"{byteData} - {stringData} - {DateTime.UtcNow}");
                OnMessageReceived(clientId, message);
            }
            WriteLine("stopped listening on client xyz...");
        }

        private async Task PrintDiagnostics(CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                builder.AppendLine($"Diagnostics: {DateTime.UtcNow}----------------------------");
                foreach (var client in _clients)
                {
                    builder.AppendLine($"client '{client.Key}': {client.Value.Connected}\t{client.Value.Client.Connected}\t{client.Value.Client.Ttl}");
                }
                OnDiagnosticRun(builder.ToString());
                builder.Clear();
            }
        }

        protected virtual void OnClientConnected(string clientId) => ClientConnected?.Invoke(clientId);
        protected virtual void OnMessageReceived(string clientId, string message) => MessageReceived?.Invoke(clientId, message);
        protected virtual void OnStarted(string endpoint) => Started?.Invoke(endpoint);
        protected virtual void OnStopped() => Stopped?.Invoke();
        protected virtual void OnStartedDiagnostics() => DiagnosticsStarted?.Invoke();
        protected virtual void OnStoppedDiagnostics() => DiagnosticsStopped?.Invoke();
        protected virtual void OnDiagnosticRun(string diagnostics) => DiagnosticRun?.Invoke(diagnostics);
    }
}