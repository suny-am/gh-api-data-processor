using GithubAPI.Library.GraphQL.Requests.Fragments;



namespace GithubDataProcessor;

public class RepositoryQuery
{
  private string _query;
  private object _variables;
  private RepositoryFragment _repositoryFragment = new();

  public RepositoryQuery()
  {
    _query = @"query ($queryString: String! $count: Int!, $type: SearchType!) {
                        search(query: $queryString, type: $type, first: $count) {
                edges {
                  node {"
                + _repositoryFragment.Name +
               @"}
                }
              }
            }" + _repositoryFragment.Value;
    _variables = new

    {
      queryString = "org:sunyam-lexicon-2024",
      count = 100,
      type = "REPOSITORY"
    };
  }
  public string Query => _query;
  public object Variables => _variables;
}