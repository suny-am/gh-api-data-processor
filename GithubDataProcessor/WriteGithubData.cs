using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using GithubAPI.Library.GraphQL.Records;
using GithubAPI.Library.GraphQL;
using CosmosDBProcessor.Library;

namespace GithubDataProcessor
{
    public class WriteGithubData(ILoggerFactory loggerFactory)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<WriteGithubData>();
        private readonly HttpClient _tokenClient = new();
        private CosmosDbHandler _cosmosDBhandler = new();
        private GraphQLHandler _graphQLHandler = null!;

        [Function("WriteGithubData")]
        public async Task Run([TimerTrigger("0 0 20 * * 0,1,2,3,4")] TimerInfo myTimer)
        {
            _logger.LogInformation("{Message}", $"C# Timer trigger function executed at: {DateTime.Now}");

            string token = await _tokenClient.GetStringAsync(Environment.GetEnvironmentVariable("GITHUB_TOKEN_ENDPOINT"));

            _graphQLHandler = new(token);

            IEnumerable<Repository>? repositoryData = await _graphQLHandler.GetRepositoriesBySearch();

            if (repositoryData is null)
            {
                _logger.LogError("could not load data: {repositoryData}", repositoryData);
            }

            // Load Data Source
            await _cosmosDBhandler.LoadDataSource("gh-api-data", "Repositories", "/name");

            //  write 
            foreach (var r in repositoryData!)
            {
                Console.WriteLine(r);
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation("{Message}", $"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
