using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using GithubAPI.Library.GraphQL;
using GithubAPI.Library.GraphQL.Requests;
using CosmosDBProcessor.Library;
using GithubAPI.Library.GraphQL.Types;

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

                var repoQuery = new RepositoryQuery();

                var query = repoQuery.Query;
                var variables = repoQuery.Variables;

                AuthenticatedRequest request = new(query, variables);

                ResponseType? response = await _graphQLHandler
                                                .PerformQuery(request) 
                                                ?? throw new Exception("Query failed");

                IEnumerable<RepositoryType?>? repositories = response?
                                                            .Search?
                                                            .Edges?
                                                            .Select(e => e.Node)
                                                            ?? throw new Exception("Could not load repositories");

                await _cosmosDBhandler.LoadDataSource("gh-api-data", "Repositories", "/name");

                IEnumerable<RepositoryItem> repositoryItems = repositories!
                                                            .Select(r => new RepositoryItem(
                                                                                            id: r!.ID!,
                                                                                            name: r.Name!,
                                                                                            homepageUrl: r.HomepageURL!,
                                                                                            url: r.URL,
                                                                                            pushedAt: r.PushedAt,
                                                                                            r.DiskUsage,
                                                                                            commitTotal: r.DefaultBranchRef!.Target!.History!.TotalCount,
                                                                                            description: r.Description!
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
