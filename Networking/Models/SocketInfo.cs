using System;
using System.Net;
using System.Net.Sockets;

namespace Networking.Models
{
    public class SocketInfo
    {
        public string Id { get; set; }
        public TheClient Client { get; set; }
        public EndPoint LocalEndPoint { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
        public bool IsConnected => Client?.TcpClient.Connected ?? false;

        public SocketInfo(string id, TheClient tcpClient)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Client = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            LocalEndPoint = tcpClient.TcpClient.Client.LocalEndPoint;
            RemoteEndPoint = tcpClient.TcpClient.Client.RemoteEndPoint;
        }
    }
}