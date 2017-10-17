using Networking.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Networking
{
    public class ClientList : IEnumerable<TheClient>
    {
        private readonly List<TheClient> _clients = new List<TheClient>();

        public void Add(string clientId, TheClient theClient)
        {
            _clients.Add(theClient);
        }

        public void Clear()
        {
            _clients.Clear();
        }

        public TheClient Get(string key)
        {
            return _clients.SingleOrDefault(c => c.Id == key);
        }

        public IEnumerable<ClientListModel> GetSerializableList()
        {
            return _clients.Where(c => c.IsConnected).Select(c => new ClientListModel
            {
                Id = c.Id,
                LocalIpAddress = c.LocalEndPoint.Address.ToString(),
                LocalPort = c.LocalEndPoint.Port,
                RemoteIpAddress = c.RemoteEndPoint.Address.ToString(),
                RemotePort = c.RemoteEndPoint.Port
            });
        }

        public IEnumerator<TheClient> GetEnumerator()
        {
            return _clients.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}