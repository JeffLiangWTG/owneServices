using System.Reflection;
using CargoWise.Billing.CollectorService.Plugin;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Sql;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common;

public abstract class ElasticSearchSQLPlugin : ElasticSearchPluginBase
{
	public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
	{
		var response = Client.Sql.QueryAsync(descriptor => descriptor.Query(GetQuery(QueryName, start.ToString("O"), end.ToString("O")))).ConfigureAwait(false).GetAwaiter().GetResult();
		List<TimeStampedTransaction> recordTransactions = new List<TimeStampedTransaction>();
		try
		{
			recordTransactions = CreateTransactions(new[] { response }).ToList();
		}
		catch (Exception e)
		{
			ErrorReportingClient?.ReportToIssueManager($"Could not create billing transactions from the Sql query response", e, Logger);
		}

		foreach (var transaction in recordTransactions)
		{
			yield return transaction;
		}
	}

	protected abstract IEnumerable<TimeStampedTransaction> CreateTransactions(QueryResponse[] responses);

	protected abstract string QueryName { get; }
	protected abstract string ResourcePrefix { get; }

	protected string GetQuery(string queryName, params object[] queryParams)
	{
		Stream stream = null;
		try
		{
			queryName = $"{ResourcePrefix}{queryName}";
			if (!queryCache.TryGetValue(queryName, out string query))
			{
				stream = QueryAssembly.GetManifestResourceStream(queryName);
				using var reader = new StreamReader(stream);
				stream = null;
				query = ModifyQuery(reader.ReadToEnd());
				queryCache.TryAdd(queryName, query);
			}

			query = AddQueryParams(query, queryParams);
			return query;
		}
		finally
		{
			stream?.Dispose();
		}
	}

	protected virtual string ModifyQuery(string input) => input;

	string AddQueryParams(string query, object[] queryParams)
	{
		if (queryParams == null || queryParams.Length == 0)
		{
			return query;
		}

		int paramIndex = 0;

		return System.Text.RegularExpressions.Regex.Replace(query, @"\?", match =>
		{
			if (paramIndex < queryParams.Length)
			{
				var value = queryParams[paramIndex] is string ? $"'{queryParams[paramIndex]}'" : queryParams[paramIndex].ToString();
				paramIndex++;
				return value;
			}

			return match.Value;
		});
	}

	internal virtual Assembly QueryAssembly
	{
		get { return GetType().Assembly; }
	}

	readonly Dictionary<string, string> queryCache = new ();
}
