using ExcluSightsLibrary.DiscordServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ExclusiveInsights
{
    public class CustomerProfileIntake
    {
        private readonly ILogger<CustomerProfileIntake> _logger;
        private readonly HttpClient _httpClient;
        private readonly ISocketEngine _socketEngine;

        public CustomerProfileIntake(ILogger<CustomerProfileIntake> logger, ISocketEngine socketEngine)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _socketEngine = socketEngine;
        }
        
        [Function("CustomerProfileIntake")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            await _socketEngine.EnsureStartedAsync();
            await _socketEngine.WaitForInitialBackfillAsync(TimeSpan.FromSeconds(50));
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
