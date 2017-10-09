using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Networking
{
    public class TcpSocket
    {
        private readonly Socket _mySocket;
        private Socket _connectedSocket;

        public Socket MySocket => _mySocket;

        public TcpSocket()
        {
            _mySocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        }

        public Task<Socket> ListenAsync(string ipAddress, int port)
        {
            return Task.Run(() =>
            {
                MySocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
                MySocket.Listen(1000);
                var asyncResult = MySocket.BeginAccept(AcceptCallback, new { });
                var wasSuccess = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10000));
                if (!wasSuccess) throw new InvalidOperationException("An error occurred trying to connect to host");
                return _connectedSocket;
            });
        }

        public async Task ConnectAsync(string ipAddress, int port)
        {
            await Task.Run(() =>
            {
                var asyncResult = MySocket.BeginConnect(IPAddress.Parse(ipAddress), port, ConnectCallback, new { });
                var wasSuccess = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                if (!wasSuccess) throw new InvalidOperationException("An error occurred trying to connect to host");
            });
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            MySocket.EndConnect(asyncResult);
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            _connectedSocket = MySocket.EndAccept(asyncResult);
        }
    }
}