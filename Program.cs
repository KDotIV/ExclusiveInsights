using ExcluSightsLibrary.DiscordServices;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((ctx, ConfigurationBuilder) =>
    {
        ConfigurationBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        ConfigurationBuilder.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        IConfiguration config = ctx.Configuration;
        services.AddSingleton<IConfiguration>(config);

        string botToken = config["DiscordBotToken"];
        string connectionString = config["PG_ConnectionString"];

        //Singletons
        services.AddSingleton<ISocketEngine, DiscordSocketEngine>(provider => 
        {
            var logger = provider.GetRequiredService<ILogger<DiscordSocketEngine>>();
            return new DiscordSocketEngine(botToken!, connectionString!, logger);
        });
    })
    .Build();
host.Run();