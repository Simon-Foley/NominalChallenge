using Client;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ClientWorker>();

var host = builder.Build();
host.Run();
