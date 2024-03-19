using Client;

public static class Program
{
    public static void Main(string[] args)
    {
        ClientHostBuilder(args)
            .Build()
            .Run();
    }

    private static HostApplicationBuilder ClientHostBuilder(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<ClientWorker>();
        return builder;
    }

}