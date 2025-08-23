using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        string connectionString = config[""];
    })
    .Build();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           