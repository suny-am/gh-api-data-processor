using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CosmosDBProcessor.Library;
using GithubAPI.Library.GraphQL;

namespace GithubDataProcessor
{
    public class ReadGithubData
    {
        private readonly ILogger _logger;
        private readonly HttpClient _tokenClient = new();
        private readonly CosmosDbHandler _cosmosDBhandler = new();
        private GraphQLHandler _graphQLHandler = null!;

        public ReadGithubData(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReadGithubData>();
        }

        [Function("ReadGithubData")]
        public async Task Run([TimerTrigger("0 0 22 * * 0,1,2,3,4")] TimerInfo myTimer)
        {
            _logger.LogInformation("{Message}", $"C# Timer trigger function executed at: {DateTime.Now}");

            string token = await _tokenClient.GetStringAsync(Environment.GetEnvironmentVariable("GITHUB_TOKEN_ENDPOINT"));

            _graphQLHandler = new(token);

            IEnumerable<Repository> repositoryData = await _graphQLHandler.GetRepositoriesBySearch();

            if (repositoryData is null)
            {
                _logger.LogError("could not load data: {repositoryData}", repositoryData);
            }

            // Load Data Source
            await _cosmosDBhandler.LoadDataSource("gh-api-data", "Repositories", "/name");

            foreach (var r in repositoryData)
            {
                System.Console.WriteLine(r.);
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation("{Message}", $"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
