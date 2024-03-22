using System.Collections.Concurrent;

namespace Master
{
    public class ClientManager
    {
        private readonly ConcurrentDictionary<Guid, ClientHandler> _clients = new();

        public void AddClient(ClientHandler clientHandler)
        {
            _clients.TryAdd(clientHandler._clientId, clientHandler);
        }


        public void RemoveClient(Guid clientId)
        {
            _clients.TryRemove(clientId, out _);
        }

        public IEnumerable<ClientHandler> GetClients()
        {
            return _clients.Values;
        }

        public int ConnectedClientsCount()
        {
            return _clients.Count;
        }
    }
}
