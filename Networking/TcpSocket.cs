using Networking.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
    public class TcpSocket
    {
        private readonly Socket _mySocket;
        private readonly Dictionary<string, SocketInfo> _sockets = new Dictionary<string, SocketInfo>();
        private CancellationTokenSource _listenCts;
        public string _ipAddress { get; set; }
        public int _port { get; set; }

        //server events
        public event Action<string, int> Started;
        public event Action<string, int> Stopped;
        public event Action<string> ClientAccepted;

        //client events
        public event Action<string, int> Connected;
        public event Action<string, int> Disconnected;
        public event Action ConnectionChanged;

        public IPEndPoint LocalEndPoint => (IPEndPoint)_mySocket.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => (IPEndPoint)_mySocket.RemoteEndPoint;

        public TcpSocket(string ipAddress, int port)
        {
            _mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            _port = port > 0 && port < 65536 ? port : throw new ArgumentOutOfRangeException(nameof(port));
        }

        public Task ListenAsync()
        {
            if (_listenCts != null && !_listenCts.IsCancellationRequested) throw new InvalidOperationException("Server is already listening");
            _listenCts = new CancellationTokenSource();
            return Task.Run(() =>
            {
                _mySocket.Bind(new IPEndPoint(IPAddress.Parse(_ipAddress), _port));
                _mySocket.Listen(1000);
                OnStarted(_ipAddress, _port);
                while (!_listenCts.IsCancellationRequested)
                {
                    var asyncResult = _mySocket.BeginAccept(AcceptCallback, new { });
                    var wasSuccess = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10000));
                    if (!wasSuccess) throw new InvalidOperationException("An error occurred accepting a client");
                }
                OnStopped(_ipAddress, _port);
                _listenCts = null;
            });
        }

        public async Task ConnectAsync()
        {
            await Task.Run(() =>
            {
                var asyncResult = _mySocket.BeginConnect(IPAddress.Parse(_ipAddress), _port, ConnectCallback, new { IpAddress = _ipAddress, Port = _port });
                var wasSuccess = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                if (!wasSuccess) throw new InvalidOperationException("An error occurred trying to connect to host");
            });
        }

        public async Task DisonnectAsync()
        {
            await Task.Run(() =>
            {
                var asyncResult = _mySocket.BeginDisconnect(false, DisconnectCallback, new { IpAddress = RemoteEndPoint.Address.ToString(), Port = RemoteEndPoint.Port });
                var wasSuccess = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                if (!wasSuccess) throw new InvalidOperationException("An error occurred trying to connect to host");
            });
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            _mySocket.EndConnect(asyncResult);
            dynamic asyncState = asyncResult.AsyncState;
            OnConnected(asyncState.IpAddress, asyncState.Port);
        }

        private void DisconnectCallback(IAsyncResult asyncResult)
        {
            _mySocket.EndDisconnect(asyncResult);
            dynamic asyncState = asyncResult.AsyncState;
            OnDisconnected(asyncState.IpAddress, asyncState.Port);
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
            //var clientId = Guid.NewGuid().ToString();
            //var socket = _mySocket.EndAccept(asyncResult);
            //_sockets.Add(clientId, new SocketInfo(clientId, socket));
            //OnClientAccepted(clientId);
        }

        protected virtual void OnClientAccepted(string clientId) => ClientAccepted?.Invoke(clientId);
        protected virtual void OnStarted(string ipAddress, int port) => Started?.Invoke(ipAddress, port);
        protected virtual void OnStopped(string ipAddress, int port) => Stopped?.Invoke(ipAddress, port);

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
    }
}