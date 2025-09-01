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

        //SINGLETONS
        services.AddSingleton<ISocketEngine, DiscordSocketEngine>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<DiscordSocketEngine>>();
            var customerIntake = provider.GetRequiredService<ICustomerIntakeService>();
            var customerQuery = provider.GetRequiredService<ICustomerQueryService>();
            return new DiscordSocketEngine(botToken!, connectionString!, logger, customerIntake, customerQuery);
        });

        //SCOPES
        services.AddScoped<ICustomerQueryService, CustomerQueryService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ICustomerQueryService>>();
            return new CustomerQueryService(connectionString!, logger);
        });
        services.AddScoped<ICustomerIntakeService, CustomerIntakeService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CustomerIntakeService>>();
            var customerQuery = provider.GetRequiredService<ICustomerQueryService>();
            return new CustomerIntakeService(connectionString!, logger, customerQuery);
        });
    })
    .Build();
host.Run();