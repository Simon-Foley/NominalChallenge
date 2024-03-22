using Master.Models;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace Master
{
    /// <summary>
    /// Represents one connected client. These are all managed by the clientManager  
    /// </summary>
    public class ClientHandler
    {
        public Guid _clientId { get; } = Guid.NewGuid();
        private readonly TcpClient _client;
        private readonly ILogger _logger;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public ClientHandler(TcpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
            var networkStream = _client.GetStream();

            _reader = new StreamReader(networkStream, Encoding.UTF8);
            _writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };

        }

        public async Task<WorkItemResult> ExecuteDecrypt(WorkItem workItem)
        {
            if (_client.Connected)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                string json = JsonConvert.SerializeObject(workItem, settings);
                _logger.LogInformation($"Attempting to write to client {_clientId} for file {workItem.FileName}");

                await _writer.WriteLineAsync(json);
                _logger.LogInformation($"Message written to client {_clientId}, awaiting response...");

                var response = await _reader.ReadLineAsync();


                if (response != null)
                {
                    try
                    {

                        var result = JsonConvert.DeserializeObject<WorkItemResult>(response);
                        _logger.LogInformation($"Received response from client {_clientId} for {result.FileName}");

                        return result;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Failed to deserialize the response from {workItem.FileName} into WorkItemResult: {ex.Message}");
                        throw;
                    }
                }
                else
                {
                    throw new Exception("No response received from client.");
                }
            }
            else
            {
                // Disconnect and remove the client since it's not connected
                Disconnect();
                throw new ClientNotConnectedException(_clientId.ToString());
            }
        }
        public void Disconnect()
        {
            try
            {
                _writer?.Close();
                _reader?.Close();
                _client?.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while disconnecting client {_clientId}: {ex.Message}");
            }
            finally
            {
                _logger.LogInformation($"Client {_clientId} disconnected.");
            }
        }
        public class ClientNotConnectedException(string id)
            : Exception(String.Format("Client not connected. Client ID: {0}", id)) { }
    }


}
