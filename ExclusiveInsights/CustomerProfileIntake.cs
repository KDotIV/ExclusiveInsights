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

        public CustomerProfileIntake(ILogger<CustomerProfileIntake> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }
        
        [Function("CustomerProfileIntake")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");



            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
