using System.Net.Sockets;
using System.Text;
using Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Client.Decryptors;

namespace Client
{
    public class ClientWorker(ILogger<ClientWorker> logger, IConfiguration configuration) : BackgroundService
    {
        private readonly string _masterhost = configuration["MASTER_HOST"]; 
        private readonly int _masterPort = int.Parse(configuration["MASTER_PORT"]);
        private readonly MessageHandler _messageHander = new MessageHandler(logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Giving time to initialise...");
            await Task.Delay(5000);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("ClientWorker attempting to connect to master...");
                    using var client = new TcpClient();
                    await client.ConnectAsync(_masterhost, _masterPort, stoppingToken);
                    logger.LogInformation("Connected to MasterWorker.");

                    using var networkStream = client.GetStream();
                    var encodingWithoutBom = new UTF8Encoding(false);
                    var reader = new StreamReader(networkStream, encodingWithoutBom);
                    var writer = new StreamWriter(networkStream, encodingWithoutBom) { AutoFlush = true };

                    while (!stoppingToken.IsCancellationRequested && client.Connected)
                    {
                        var message = await reader.ReadLineAsync();

                        if (message != null)
                        {
                            var workItemResult = _messageHander.HandleMessage(message);
                            var serializedResult = JsonConvert.SerializeObject(workItemResult);
                            await writer.WriteLineAsync(serializedResult);
                            logger.LogInformation("Sent result back to Master");
                        }
                        else
                        {
                            logger.LogWarning("Connection lost, attempting to reconnect...");
                            break; // Exit the inner while loop to attempt reconnection
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"An error occurred: {ex.Message}");
                }

                // Try reconnect to master
                await Task.Delay(5000, stoppingToken);
            }
        }

        private WorkItemResult Decrypt(WorkItem workItem)
        {
            logger.LogInformation($"Decrypting WorkItem: {workItem.FileName}");

            var sampleObject = new JObject();

            var decryptedText = "This has now been decrypted: " + workItem.EncryptedText;

            sampleObject["test"] = decryptedText;
            var result = new WorkItemResult(workItem.FileName, Result.Succeeded, sampleObject);
                return result;
        }
    }
}
