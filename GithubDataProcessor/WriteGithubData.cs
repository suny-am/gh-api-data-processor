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
            _logger.LogInformation("{Message}",
                                    $"Write operations from" +
                                    $"Github to CosmosDB started at: {DateTime.Now}");

            bool operationCompleted = false;
            try
            {
                string ghApiToken = await _tokenClient.GetStringAsync(Environment
                                                        .GetEnvironmentVariable("GITHUB_TOKEN_ENDPOINT"));

                _graphQLHandler = new(ghApiToken);

                IEnumerable<Repository>? repositoryData = await _graphQLHandler
                                                                    .GetRepositoriesBySearch();

                await _cosmosDBhandler.LoadDataSource("gh-api-data", "Repositories", "/name");

                IEnumerable<RepositoryItem> repositoryItems = repositoryData!.Select(r => new RepositoryItem(
                                                                                                r.id,
                                                                                                r.name,
                                                                                                r.homepageUrl,
                                                                                                r.url,
                                                                                                r.pushedAt,
                                                                                                r.diskUsage,
                                                                                                r.commitTotal,
                                                                                                r.description
                                                                                                ));

                await _cosmosDBhandler.UnitOfWork(repositoryItems, "CREATE");
                operationCompleted = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", $"Could not complete write operations: {ex.Message}");
            }
            finally
            {
                string operationStatus = operationCompleted ? "completed" : "failed";

                if (myTimer.ScheduleStatus is not null)
                {
                    _logger.LogInformation("{Message}", $" Operation {operationStatus}. " +
                    $"Next timer scheduled to: {myTimer.ScheduleStatus.Next}");
                }
            }
        }
    }
}
