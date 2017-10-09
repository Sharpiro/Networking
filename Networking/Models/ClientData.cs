using System;
using System.Net;
using System.Net.Sockets;

namespace Networking.Models
{
    public class ClientData
    {
        public string Id { get; set; }
        public TcpClient TcpClient { get; set; }
        public EndPoint LocalEndPoint { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
        public bool Connected => TcpClient?.Connected ?? false;

        public ClientData()
        {

        }

        public ClientData(string id, TcpClient tcpClient)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            TcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            LocalEndPoint = tcpClient.Client.LocalEndPoint;
            RemoteEndPoint = tcpClient.Client.RemoteEndPoint;
        }
    }
}