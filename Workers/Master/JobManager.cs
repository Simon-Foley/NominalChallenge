using Master.Models;
using System.Collections.Concurrent;
using static Master.ClientHandler;

namespace Master
{
    public class JobManager
    {
        private readonly ClientManager _clientManager;
        private ConcurrentQueue<WorkItem> _workItemsQueue = new ConcurrentQueue<WorkItem>();

        public JobManager(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public void QueueWorkItems(List<WorkItem> workItems)
        {
            foreach (var workItem in workItems)
            {
                _workItemsQueue.Enqueue(workItem);
            }
        }

        public async Task<List<WorkItemResult>> ExecuteBatchJob()
        {
            var results = new ConcurrentBag<WorkItemResult>();
            var clientTasks = new List<Task>();

            // ToList() so that we can remove _clients from the actual dict if they aren't connected
            // haven't tested that
            foreach (var client in _clientManager.GetClients().ToList())
            {
                clientTasks.Add(Task.Run(async () =>
                {
                    while (_workItemsQueue.TryDequeue(out var workItem))
                    {
                        try
                        {
                            var result = await client.ExecuteDecrypt(workItem);
                            results.Add(result);
                        }
                        catch (ClientNotConnectedException)
                        {
                            _clientManager.RemoveClient(client._clientId);
                        }
                    }

                }));
            }

            await Task.WhenAll(clientTasks);

            return results.ToList();
        }
    }
}
