using CargoWise.Billing.CollectorService.Plugin;
using Elastic.Clients.Elasticsearch;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common;

public abstract class ElasticSearchPlugin<TDocument> : ElasticSearchPluginBase where TDocument : class
{
	public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
	{
		var client = CreateElasticClient();
		var response = client.SearchAsync<TDocument>(GetSelector(start, end)).ConfigureAwait(false).GetAwaiter().GetResult();
		List<TimeStampedTransaction> recordTransactions = new List<TimeStampedTransaction>();
		try
		{
			recordTransactions = CreateTransactions(response).ToList();
		}
		catch (Exception e)
		{
			ErrorReportingClient?.ReportToIssueManager($"Could not create billing transactions from the search response", e, Logger);
		}

		return recordTransactions;
	}

	public abstract IEnumerable<TimeStampedTransaction> CreateTransactions(SearchResponse<TDocument> searchResponse);

	public abstract SearchRequest GetSelector(DateTime start, DateTime end);
}
