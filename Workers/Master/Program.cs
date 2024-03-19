using Master;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Master>();

var host = builder.Build();
host.Run();
