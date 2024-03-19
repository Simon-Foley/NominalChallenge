using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace Master
{
    public class MasterWorker : BackgroundService
    {
        private readonly ILogger<MasterWorker> _logger;

        public MasterWorker(ILogger<MasterWorker> logger)
        {
            _logger = logger;
            _webHost = new WebHostBuilder()
                .UseKestrel()
                .Configure(app => app.Run(async context =>
                {
                    await context.Response.WriteAsync("Hello from Master Worker!");
                }))
                .UseUrls("http://*:4000") // Listen on port 4000
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
