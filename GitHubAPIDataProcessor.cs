using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace gh_data_collector
{
    public class GitHubAPIDataProcessor
    {
        private readonly ILogger _logger;

        public GitHubAPIDataProcessor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GitHubAPIDataProcessor>();
        }

        [Function("GitHubAPIDataProcessor")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
