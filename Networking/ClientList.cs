using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Networking
{
    public class ClientList : IEnumerable<TheClient>
    {
        private readonly Dictionary<string, TheClient> _clients = new Dictionary<string, TheClient>(StringComparer.InvariantCultureIgnoreCase);

        public void Add(string clientId, TheClient theClient)
        {
            _clients.Add(clientId, theClient);
        }

        public void Clear()
        {
            _clients.Clear();
        }

        public TheClient Get(string key)
        {
            var itemExists = _clients.TryGetValue(key, out TheClient client);
            return itemExists ? client : default;
        }

        public object GetSerializableList()
        {
            return _clients.Select(c => new
            {
                Id = c.Key,
                LocalIpAddress = c.Value.LocalEndPoint.Address.ToString(),
                LocalPort = c.Value.LocalEndPoint.Port,
                RemoteIpAddress = c.Value.RemoteEndPoint.Address.ToString(),
                RemotePort = c.Value.RemoteEndPoint.Port
            });
        }

        public IEnumerator<TheClient> GetEnumerator()
        {
            return _clients.Select(c => c.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}