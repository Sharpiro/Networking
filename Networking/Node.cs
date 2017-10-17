using Networking.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public class Node
    {
        public Server Server { get; }
        public TheClient SuperNodeConnection { get; }
        public ClientList Clients { get; } = new ClientList();

        public Node(Server server, TheClient superNodeConnection)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            SuperNodeConnection = superNodeConnection ?? throw new ArgumentNullException(nameof(superNodeConnection));
        }

        public async Task ConnectToSuperNodeAndListen()
        {
            SuperNodeConnection.CommandReceived += HandleCommand;
            //Server.

            await SuperNodeConnection.ConnectAsync();

            var localIp = SuperNodeConnection.LocalEndPoint.Address.ToString();
            Server.IpAddress = localIp;
            Server.Port = SuperNodeConnection.LocalEndPoint.Port;
            var _ = Server.ListenAsync();
        }

        private async void HandleCommand(SocketMessage socketMessage)
        {

            if (socketMessage.Title.ToLowerInvariant() == nameof(ConnectToPeers).ToLowerInvariant())
                await ConnectToPeers(socketMessage);
        }

        private async Task ConnectToPeers(SocketMessage socketMessage)
        {
            var jsonString = Encoding.UTF8.GetString(socketMessage.Data);
            var peerData = JsonConvert.DeserializeObject<List<ClientListModel>>(jsonString);

            //var otherClients = peerData.Where(c => c.Id != SuperNodeConnection.Id);
            //if (otherClient == null) return;
            //foreach (var peer in peerData)
            //{
            //    if (peer.RemotePort == Server.Port)
            //    {
            //        continue;
            //    }

            //    var client = new TheClient(peer.RemoteIpAddress, peer.RemotePort);
            //    await client.ConnectAsync();
            //    Clients.Add(null, client);
            //}
        }
    }
}