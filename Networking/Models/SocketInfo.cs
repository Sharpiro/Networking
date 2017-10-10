using System;
using System.Net;
using System.Net.Sockets;

namespace Networking.Models
{
    public class SocketInfo
    {
        public string Id { get; set; }
        public Socket TcpClient { get; set; }
        public EndPoint LocalEndPoint { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
        public bool Connected => TcpClient?.Connected ?? false;

        public SocketInfo(string id, Socket socket)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            TcpClient = socket ?? throw new ArgumentNullException(nameof(socket));
            LocalEndPoint = socket.LocalEndPoint;
            RemoteEndPoint = socket.RemoteEndPoint;
        }
    }
}