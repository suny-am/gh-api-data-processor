using System;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace gh_data_collector
{
    public class GitHubAPIDataProcessor
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _client;

        public GitHubAPIDataProcessor(ILoggerFactory loggerFactory)
        {
            var credential = new DefaultAzureCredential();
            _logger = loggerFactory.CreateLogger<GitHubAPIDataProcessor>();
            _client = new(
                accountEndpoint: Environment.GetEnvironmentVariable("COSMOS_ENDPOINT"),
                tokenCredential: credential
            );
        }

        [Function("GithubAPIWrite")]
        // Run trigger at 22:00
        public void Run([TimerTrigger("0 0 22 * * *")] TimerInfo myTimer)
        {

            // Create DB if not exists

            // Create Container if not exists

            // Create Item if not exists

            _logger.LogInformation("{Message}", $"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation("{Message}", $"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
