using Master.Models;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;


namespace Master
{
    public class MasterWorker(ILogger<MasterWorker> logger) : BackgroundService
    {

        private readonly ClientManager _clientManager = new();

        /// <summary>
        /// Start of the main Master worker action. Starts the connections with the clients, 
        /// loads the files and sends them to the job manager which gives back the responses when they're all done.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("MasterWorker starting TCP server...");
            StartServer(4000, stoppingToken);

            logger.LogInformation("Waiting for client connections...");
            
            // Technically we don't need to wait for 2 it can run with 1
            // but this guarantees we get both (could depend on env variables for replica too)
            while (_clientManager.ConnectedClientsCount() < 2)
            {
                logger.LogInformation("Waiting for clients");
                await Task.Delay(1000); // Check every 1 second for a client connection
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Stopping as cancellation was requested.");
                    return;
                }
            }

            logger.LogInformation("Clients connected. Proceeding...");

            var workItems = LoadInputs();

            var jobManager = new JobManager(_clientManager);
            jobManager.QueueWorkItems(workItems);


            logger.LogInformation("Executing Batch Job");
            var results = await jobManager.ExecuteBatchJob();

            foreach(WorkItemResult result in results)
            {
                logger.LogInformation($"Result for file: {result.FileName} \n {result.JsonResult?.ToString() ?? result.ErrorMessage}");
                //logger.LogInformation(result.JsonResult?.ToString() ?? result.ErrorMessage);
            }

            WriteResultsToOutput(results);
            logger.LogInformation("Done :)");
        }

        private async Task StartServer(int port, CancellationToken stoppingToken)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, port);
            var listener = new TcpListener(localEndPoint);
            listener.Start();
            logger.LogInformation("TCP server started. Waiting for connections...");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var clientSocket = await listener.AcceptTcpClientAsync();

                    // Initialize ClientHandler with TcpClient
                    var clientHandler = new ClientHandler(clientSocket, logger);
                    logger.LogInformation($"Client connected. ID: {clientHandler._clientId}");

                    _clientManager.AddClient(clientHandler);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An exception occurred while running the server: {ex.Message}");
            }
        }

        public static List<WorkItem> LoadInputs()
        {
            var inputDirPath = "/app/inputs";
            var inputFiles = Directory.GetFiles(inputDirPath);

            // Filename, contents
            var workItems = new List<WorkItem>();

            foreach (var filePath in inputFiles)
            {
                // Read bytes so we dont get tripped up by weird characters
                var fileContent = File.ReadAllBytes(filePath);
                // Convert byte array to Base64 string for json serialisation
                var base64EncryptedData = Convert.ToBase64String(fileContent);

                var fileName = Path.GetFileName(filePath);
                workItems.Add(new WorkItem(fileName, base64EncryptedData));
            }

            return workItems;
        }
        private void WriteResultsToOutput(IEnumerable<WorkItemResult> results)
        {
            var outputDirPath = "/app/outputs";
            if (!Directory.Exists(outputDirPath))
            {
                Directory.CreateDirectory(outputDirPath);
            }

            foreach (var result in results)
            {
                var outputFilePath = Path.Combine(outputDirPath, $"result_{result.FileName}");
                var contentToWrite = result.JsonResult != null ?
                             result.JsonResult.ToString() :
                             result.ErrorMessage;

                File.WriteAllText(outputFilePath, contentToWrite);
                logger.LogInformation($"Wrote results to {outputFilePath}");
            }
        }
    }
}