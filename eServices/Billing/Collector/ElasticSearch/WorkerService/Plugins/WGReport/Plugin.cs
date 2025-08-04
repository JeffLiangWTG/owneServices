using System.Diagnostics;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.MSearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Newtonsoft.Json;
using SourceFilter = Elastic.Clients.Elasticsearch.Core.Search.SourceFilter;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WGReport
{
	public class Plugin : ElasticSearchPluginBase
	{
                public override void UpdateSettings(PluginSettings settings)
                {
                        base.UpdateSettings(settings);
                        CpuMonitoringIndex = settings.Parameters.FirstOrDefault(x => x.Name.Equals("CpuMonitoringIndex"))?.Value ?? string.Empty;
                        GatewayNonBilledUsage = settings.Parameters.FirstOrDefault(x => x.Name.Equals("GatewayNonBilledUsage"))?.Value ?? string.Empty;
			ElasticRetryMaxAttempts = GetIntParameter(settings, "ElasticRetryMaxAttempts", 1440);
			ElasticRetryDelayInSecond = GetIntParameter(settings, "ElasticRetryDelayInSecond", 60);
                        BatchSize = GetIntParameter(settings, "BatchSize", 20);

                        var jsonString = settings.Parameters.FirstOrDefault(x => x.Name.Equals("DefectiveQueryHashesDictionary"))?.Value;
			DefectiveQueryHashesDictionary = string.IsNullOrWhiteSpace(jsonString)
				? new Dictionary<string, string[]>()
				: JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonString) ?? new Dictionary<string, string[]>();
		}


		public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
		{
			var systems = GetClientSystems(start, end, Client);
			var totalSystems = systems.Count();
			var semaphore = new SemaphoreSlim(BatchSize > 0 ? BatchSize : 20);
			var tasks = new List<Task<(string EnterpriseCode, string ServerCode, IEnumerable<TimeStampedTransaction> Transactions)>>();
			int currentIndex = 0;

			foreach (var system in systems)
			{
				currentIndex++;
				Logger.LogInformation($"Processing system {currentIndex}/{totalSystems}: EnterpriseCode={system.EnterpriseCode}, ServerCode={system.ServerCode}");
				semaphore.Wait();

				var task = Task.Run(async () =>
				{
					var stopwatch = Stopwatch.StartNew();

					try
					{
						var list = new List<TimeStampedTransaction>();
						await foreach (var tx in RetrieveAndYieldPerTransactionAsync(system.EnterpriseCode, system.ServerCode, start, end))
						{
							list.Add(tx);
						}
						return (system.EnterpriseCode, system.ServerCode, list.AsEnumerable());
					}
					finally
					{
						stopwatch.Stop();
						Logger.LogInformation($"Finished system {system.EnterpriseCode}/{system.ServerCode} in {stopwatch.Elapsed.TotalSeconds:F2}s");
						semaphore.Release();
					}
				});

				tasks.Add(task);

				if (tasks.Count >= BatchSize)
				{
					var finished = Task.WhenAny(tasks).Result;
					tasks.Remove(finished);
					foreach (var tx in finished.Result.Transactions)
					{
						yield return tx;
					}
				}
			}

			foreach (var remaining in tasks)
			{
				var result = remaining.Result;
				foreach (var tx in result.Transactions)
				{
					yield return tx;
				}
			}
		}

		private async IAsyncEnumerable<TimeStampedTransaction> RetrieveAndYieldPerTransactionAsync(string enterpriseCode, string serverCode, DateTime startUtc, DateTime endUtc)
		{
			Logger.LogInformation($"Retrieving CPU usage for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");

			var excludingIds = await RetryAsync(() => GetExcludingTransactionIds(Client, enterpriseCode, serverCode, startUtc, endUtc, DefectiveQueryHashesDictionary), ElasticRetryMaxAttempts, ElasticRetryDelayInSecond);
			Logger.LogInformation($"Retrieving highest cpu info for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");
			var highestCpuUsage = await RetryAsync(() => GetHighestCpuUsage(enterpriseCode, serverCode, startUtc, endUtc, excludingIds.ToList(), Client), ElasticRetryMaxAttempts, ElasticRetryDelayInSecond);
			if (highestCpuUsage == null)
			{
				yield break;
			}

			Logger.LogInformation($"Retrieved highest cpu info for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");
			Logger.LogInformation($"Retrieving max cpu cores info for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");
			var highestCpuCores = await RetryAsync(() => GetHighestCpuCores(enterpriseCode, serverCode, startUtc, endUtc, excludingIds.ToList(), Client), ElasticRetryMaxAttempts, ElasticRetryDelayInSecond);
			Logger.LogInformation($"Retrieved max cpu cores info for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");

			var userCount = await RetryAsync(() => GetUserCount(enterpriseCode, serverCode, startUtc, endUtc, Client), ElasticRetryMaxAttempts, ElasticRetryDelayInSecond);
			Logger.LogInformation($"{userCount} users for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");

			var transactions = CreateTransactions(
				highestCpuUsage.EnterpriseCode,
				highestCpuUsage.ServerCode,
				highestCpuUsage.Reference1,
				highestCpuUsage.Reference2,
				highestCpuUsage.Reference3,
				highestCpuUsage.Reference4,
				highestCpuUsage.Reference5,
				highestCpuUsage.ServiceOccuredUTC,
				highestCpuCores ?? 0,
				userCount ?? 0);

			foreach (var tx in transactions)
			{
				yield return tx;
				Logger.LogInformation($"Yielded transaction for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");
			}
		}
        public IEnumerable<(string EnterpriseCode, string ServerCode)> GetClientSystems(DateTime start, DateTime end, ElasticsearchClient client)
        {
			Logger.LogInformation("Retrieving systems");

			var response = client.SearchAsync<DynamicResponse>(s => s
				.Index(CpuMonitoringIndex)
				.Size(10000)
				.Query(q => q
					.Bool(b => b
						.Filter(f => f
							.Range(r => r
								.DateRange(dr => dr
									.Field("@timestamp")
									.Gte(start)
									.Lte(end)
								))
						)
					)
				)
			).GetAwaiter().GetResult();
			var querySystemsResponse = client.SearchAsync<(string EnterpriseCode, string ServerCode)>(s => s
				.Query(q => q
					.Bool(b => b
						.Must(
							m => m.Term(t => t
								.Field("UsageCode.keyword"!)
								.Value("WGR")),
							m => m.Term(t => t
								.Field("Category.keyword"!)
								.Value("WGR")))
						.MustNot(mn => mn.Term(t => t
							.Field("ServerCode.keyword"!)
							.Value("TRN")))
						.Filter(filter => filter.Range(
							range => range.DateRange(
								dateRange => dateRange
									.Field("@timestamp"!)
									.Gte(start)
									.Lte(end)))))).Aggregations(descriptor => descriptor
					.Add("system_group", aggrDescriptor => aggrDescriptor
						.Composite(compositeAggregationDescriptor => compositeAggregationDescriptor
							.Size(50000)
							.Sources(new List<IDictionary<string, CompositeAggregationSource>>()
							{
								new Dictionary<string, CompositeAggregationSource>()
								{
									{"EnterpriseCode", new (){Terms = new (){Field = "EnterpriseCode.keyword"}}}
								},
								new Dictionary<string, CompositeAggregationSource>()
								{
									{"ServerCode", new (){Terms = new (){Field = "ServerCode.keyword"}}}
								}
							}))))).ConfigureAwait(false).GetAwaiter().GetResult();

			var compositeAggregation = querySystemsResponse.Aggregations?.GetComposite("system_group");

			if (compositeAggregation?.Buckets == null)
			{
				Logger.LogWarning("No systems found in response.");
				return Enumerable.Empty<(string, string)>();
			}
			var systems = compositeAggregation.Buckets
				.Select(x => (
					EnterpriseCode: x.Key.TryGetValue("EnterpriseCode", out var enterpriseCode) && enterpriseCode.Value != null
						? enterpriseCode.Value?.ToString() ?? string.Empty : string.Empty,
					ServerCode: x.Key.TryGetValue("ServerCode", out var serverCode) && serverCode.Value != null
						? serverCode.Value?.ToString() ?? string.Empty : string.Empty
				))
				.Where(s => !string.IsNullOrEmpty(s.EnterpriseCode) && !string.IsNullOrEmpty(s.ServerCode));

			Logger.LogInformation($"Found {systems.Count()} systems");
                        return systems;
                }


		void AwaitAndProcessTransactions(List<Task<IEnumerable<TimeStampedTransaction>>> tasks, List<TimeStampedTransaction> transactions)
		{
			var transactionGroups = Task.WhenAll(tasks).Result;
			transactions.AddRange(transactionGroups.SelectMany(x => x).ToArray());
			tasks.Clear();
		}

		public async Task<int?> GetUserCount(string enterpriseCode, string serverCode, DateTime startUtc, DateTime endUtc, ElasticsearchClient client)
		{
			var userCountResponse = await client.SearchAsync<DynamicResponse>(descriptor => descriptor
					.Size(0)
					.Query(q => q
						.Bool(b => b
							.Must(
								m => m.Wildcard(
									wc => wc
										.Field("ClientID"!)
										.Value($"{enterpriseCode}*{serverCode}")
										.CaseInsensitive()),
								m => m.Match(
									match => match
										.Field("PriceItemCode.keyword"!)
										.Query("USR")),
								m => m.Match(
									match => match
										.Field("Category.keyword"!)
										.Query("STL")
								))
							.Filter(
								filter => filter.Range(
									range => range.DateRange(
										dateRange => dateRange
											.Field("@timestamp"!)
											.Gte(startUtc)
											.Lte(endUtc))))))
					.Aggregations(aggr => aggr
						.Add("user_count", aggregationDescriptor => aggregationDescriptor.Sum(sum => sum.Field("BillableCount"!)))))
				.ConfigureAwait(false);

			var userCount = (int?)userCountResponse.Aggregations?.GetSum("user_count")?.Value;
			return userCount;
		}

		public async Task<double?> GetHighestCpuCores(string enterpriseCode, string serverCode, DateTime startUtc, DateTime endUtc, List<string> excludingIds, ElasticsearchClient client)
		{
			var maxCoresResponse = await client.SearchAsync<WGRUsageTransaction>(s => s
				.Index(GatewayNonBilledUsage)
				.Size(10000)
				.Query(queryDescriptor => queryDescriptor
					.Bool(boolQueryDescriptor => boolQueryDescriptor
						.Must(
							m => m.Match(
								match => match
									.Field(f => f.EnterpriseCode)
									.Query(enterpriseCode)
							),
							m => m.Match(
								match => match
									.Field(f => f.ServerCode)
									.Query(serverCode)
							),
							m => m.Match(
								match => match
									.Field(f => f.UsageCode)
									.Query("WGR")
							),
							m => m.Match(
								match => match
									.Field(f => f.Category)
									.Query("WGR")
							))
						.MustNot(mn => mn
							.Terms(terms => terms
								.Field("_id"!)
								.Term(new TermsQueryField(excludingIds.Select(FieldValue.String).ToArray()))))
						.Filter(filter => filter.Range(
							range => range.DateRange(
								dateRange => dateRange
									.Field("@timestamp"!)
									.Gte(startUtc)
									.Lte(endUtc))))))// We don't need individual documents in the result
				.Aggregations(aggs => aggs
						.Add("group_usage", aggsGroupDescriptor => aggsGroupDescriptor
							.Composite(composite => composite
								.Size(10000)
								.Sources(new List<IDictionary<string, CompositeAggregationSource>>()
								{
									new Dictionary<string, CompositeAggregationSource>()
									{
										{"EnterpriseCode", new (){Terms = new (){Field = "EnterpriseCode.keyword"}}}
									},
									new Dictionary<string, CompositeAggregationSource>()
									{
										{"ServerCode", new (){Terms = new (){Field = "ServerCode.keyword"}}}
									},
									new Dictionary<string, CompositeAggregationSource>()
									{
										{"@timestamp", new (){Terms = new (){Field = "@timestamp"}}}
									}
								}))
							.Aggregations(sumAggs => sumAggs
								.Add("sum_usage", aggsSumDescriptor => aggsSumDescriptor
									.Sum(sum => sum
										.Script(sumScript => sumScript
											.Source("Double.parseDouble(doc['UsageCount'].value.toString()) / Integer.parseInt(doc['Reference1.keyword'].value.toString())"))))
								.Add("cpu_core_sort", aggsSortDescriptor => aggsSortDescriptor
									.BucketSort(bs => bs
										.Sort(new List<SortOptions>()
										{
											SortOptions.Field("sum_usage"!, new FieldSort()
											{
												Order = SortOrder.Desc
											})
										}))))) // Group by composite key

				)
			);

			var highestCpuCores = maxCoresResponse.Aggregations?.GetComposite("group_usage")?.Buckets.First().Aggregations.GetSum("sum_usage")?.Value;
			return highestCpuCores;
		}

		public async Task<WGRUsageTransaction?> GetHighestCpuUsage(string enterpriseCode, string serverCode, DateTime startUtc, DateTime endUtc, List<string> excludingIds, ElasticsearchClient client)
		{
			var highestCpuResponse = await client.SearchAsync<WGRUsageTransaction>(search => search
				.Index(GatewayNonBilledUsage)
				.Size(1)
				.Query(queryDescriptor => queryDescriptor
					.Bool(boolQueryDescriptor => boolQueryDescriptor
						.Must(
							m => m.Match(
								match => match
									.Field(f => f.EnterpriseCode)
									.Query(enterpriseCode)
							),
							m => m.Match(
								match => match
									.Field(f => f.ServerCode)
									.Query(serverCode)
							),
							m => m.Match(
								match => match
									.Field(f => f.UsageCode)
									.Query("WGR")
							),
							m => m.Match(
								match => match
									.Field(f => f.Category)
									.Query("WGR")
							))
						.MustNot(mn => mn
							.Terms(terms => terms
								.Field("_id"!)
								.Term(new TermsQueryField(excludingIds.Select(FieldValue.String).ToArray()))))
						.Filter(filter => filter.Range(
							range => range.DateRange(
								dateRange => dateRange
									.Field("@timestamp"!)
									.Gte(startUtc)
									.Lte(endUtc))))))
				.Sort(sortDes => sortDes
					.Script(scriptSortDes => scriptSortDes
						.Type(ScriptSortType.Number)
						.Script(s => s.Lang("painless").Source("Double.parseDouble(doc['UsageCount'].value.toString()) / Integer.parseInt(doc['Reference1.keyword'].value.toString())"))
						.Order(SortOrder.Desc)
					)
				)
			);
			var highestCpuUsage = highestCpuResponse.Documents.FirstOrDefault();
			if (highestCpuUsage == null)
			{
				Logger.LogWarning($"No CPU usage document found for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}. Skipping.");
				return null;
			}
			return highestCpuUsage;
		}

		public async Task<IEnumerable<string>> GetExcludingTransactionIds(ElasticsearchClient client, string enterpriseCode, string serverCode, DateTime startUtc, DateTime endUtc, Dictionary<string, string[]> defectiveQueryHashesDictionary)
		{
			try
			{
				var transactions = await GetWGRUsageTransactions(enterpriseCode, serverCode, startUtc, endUtc, defectiveQueryHashesDictionary, Client);
				var sqlMonitoringElasticClient = CreateElasticClient();
				return await FilterDefectiveTransactionIds(enterpriseCode, serverCode, defectiveQueryHashesDictionary, transactions,  sqlMonitoringElasticClient);
			}
			catch (Exception e)
			{
				Logger.LogError(e, "An error occured while querying the ElasticSearch");
				throw;
			}
		}

		public async Task<IEnumerable<string>> FilterDefectiveTransactionIds(string enterpriseCode, string serverCode,Dictionary<string, string[]> defectiveQueryHashesDictionary, List<WGRUsageTransaction> transactions, ElasticsearchClient sqlMonitoringElasticClient)
		{
			var searchRequests = new List<SearchRequestItem>();

			foreach (var transaction in transactions)
			{
				var defectiveSearchRangeEndUtc = transaction.ServiceOccuredUTC;
				var sampleSeconds = int.Parse(transaction.Reference1);
				var usageType = transaction.Reference2.ToLower();
				var defectiveSearchRangeStartUtc = defectiveSearchRangeEndUtc.AddSeconds(0 - sampleSeconds);
				var databaseName = $"ODYSSEY{enterpriseCode}{serverCode}";

				var searchRequest = new SearchRequestItem(
					new MultisearchHeader() { Indices = new[] { CpuMonitoringIndex } },
					new MultisearchBody()
					{
						Size = 0,
						Query = Query.Bool(new BoolQuery
						{
							Must = new List<Query>
							{
								Query.Range(new DateRangeQuery("@timestamp"!)
								{
									Gte = defectiveSearchRangeStartUtc.AddHours(-5),
									Lte = defectiveSearchRangeEndUtc.AddHours(5)
								}),
								new MatchQuery("DatabaseName.keyword"!) { Query = databaseName },
								new TermsQuery
								{
									Field = "QueryHash.keyword"!,
									Term =
										new TermsQueryField(
											defectiveQueryHashesDictionary[usageType]
												.Select(FieldValue.String).ToArray())
								},
								new ScriptQuery()
								{
									Script = new Script()
									{
										Source =
											"doc['CollectSystemTimeUtc'].value.toInstant().toEpochMilli() - doc['Duration'].value <= params.endUtc",
										Params = new Dictionary<string, object>()
										{
											{
												"endUtc",
												new DateTimeOffset(
														defectiveSearchRangeEndUtc)
													.ToUnixTimeMilliseconds()
											}
										}
									},
								},
								new BoolQuery()
								{
									Should = new List<Query>()
									{
										Query.Range(
											new DateRangeQuery("CollectSystemTimeUtc"!)
											{
												Gte = defectiveSearchRangeStartUtc,
												Lte = defectiveSearchRangeEndUtc
											}),
										Query.Range(
											new DateRangeQuery("CollectSystemTimeUtc"!)
											{
												Gt = defectiveSearchRangeEndUtc
											})
									}
								}
							}
						}),
					});

				searchRequests.Add(searchRequest);
			}

			var ids = new List<string>();
			if (searchRequests.Count > 0)
			{
				var msRequest = new MultiSearchRequest() { Searches = searchRequests, MaxConcurrentSearches = 20 };

				var msResponses = await sqlMonitoringElasticClient.MultiSearchAsync<DynamicResponse>(msRequest)
					.ConfigureAwait(false);
				for (int i = 0; i < msResponses.Responses.Count; i++)
				{
					var response = msResponses.Responses.ElementAt(i);
					if (response.Match(item => item.Total > 0, @base => true))
					{
						ids.Add(transactions[i].Id);
					}
				}

				if (ids.Count > 0)
				{
					Logger.LogInformation(
						$"Found {ids.Count} {string.Join(",", defectiveQueryHashesDictionary.Keys)} usage having defect associated with for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");
					return ids;
				}
			}

			return Array.Empty<string>();
		}

		public async Task<List<WGRUsageTransaction>> GetWGRUsageTransactions(string enterpriseCode, string serverCode, DateTime startUtc, DateTime endUtc, Dictionary<string, string[]> defectiveQueryHashesDictionary, ElasticsearchClient client)
		{
			var openPitResponse = await client.OpenPointInTimeAsync<OpenPointInTimeResponse>(d => d.Indices(GatewayNonBilledUsage).KeepAlive("5m"));
			var pitId = openPitResponse.Id;
			var wgrSearchDescriptor = new SearchRequestDescriptor<WGRUsageTransaction>().Index(GatewayNonBilledUsage)
				.Size(10000)
				.Source(new SourceConfig(new SourceFilter()
				{
					Includes = Fields.FromFields(new[]
					{
						new Field("@timestamp"!), new Field("Reference1"!), new Field("Reference2"!), new Field("SubmitToELKTime"!)
					})
				}))
				.Query(queryDescriptor => queryDescriptor
					.Bool(boolQueryDescriptor => boolQueryDescriptor
						.Must(
							m => m.Term(
								match => match
									.Field("EnterpriseCode.keyword"!)
									.Value(enterpriseCode)
							),
							m => m.Term(
								match => match
									.Field("ServerCode.keyword"!)
									.Value(serverCode)
							),
							m => m.Term(
								match => match
									.Field("UsageCode.keyword"!)
									.Value("WGR")
							),
							m => m.Term(
								match => match
									.Field("Category.keyword"!)
									.Value("WGR")
							),
							m => m.Terms(
								terms => terms
									.Field("Reference2"!)
									.Term(new TermsQueryField(defectiveQueryHashesDictionary.Keys
										.Select(FieldValue.String).ToArray()))
							))
						.Filter(filter => filter.Range(
							range => range.DateRange(
								dateRange => dateRange
									.Field("@timestamp"!)
									.Gte(startUtc)
									.Lte(endUtc))))))
				.Sort(new List<SortOptions>()
				{
					SortOptions.Field("@timestamp"!, new FieldSort()
					{
						Order = SortOrder.Asc
					}),
					SortOptions.Field("SubmitToELKTime"!, new FieldSort()
					{
						Order = SortOrder.Asc
					})
				});
			var wgrTransactionResponse = await client.SearchAsync<WGRUsageTransaction>(wgrSearchDescriptor)
				.ConfigureAwait(false);
			var transactions = new List<WGRUsageTransaction>();
			if (wgrTransactionResponse.IsValidResponse && wgrTransactionResponse.Documents.Any())
			{

				transactions = new List<WGRUsageTransaction>(wgrTransactionResponse.Hits.Select(
					h =>
					{
						if (h.Source is not null)
						{
							h.Source.Id = h.Id!.ToString();
						}

						return h.Source;
					})!);

				var lastTransaction = transactions.Last();

				while (!string.IsNullOrEmpty(pitId))
				{
					var searchResponse = await client.SearchAsync<WGRUsageTransaction>(wgrSearchDescriptor
						.Pit(p => p.Id(pitId).KeepAlive("1m"))
						.SearchAfter(new List<FieldValue>()
						{
							FieldValue.String(DateTime
								.SpecifyKind(lastTransaction.ServiceOccuredUTC, DateTimeKind.Utc).ToString("O")),
							FieldValue.String(DateTime
								.SpecifyKind(lastTransaction.SubmitToELKTime, DateTimeKind.Local)
								.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")),
						})
						.TrackTotalHits(new TrackHits(false))
					).ConfigureAwait(false);

					if (searchResponse.Documents.Any())
					{
						pitId = searchResponse.PitId ?? string.Empty;
						transactions.AddRange(searchResponse.Hits.Select(
							h =>
							{
								if (h.Source is not null)
								{
									h.Source.Id = h.Id!.ToString();
								}

								return h.Source;
							})!);
						lastTransaction = transactions.Last(); // Update for next iteration
					}
					else
					{
						break;
					}
				}
			}

			_ = await client.ClosePointInTimeAsync(c => c.Id(pitId)).ConfigureAwait(false);

			Logger.LogInformation(
				$"Searching defective queries for {transactions.Count} {string.Join(",", defectiveQueryHashesDictionary.Keys)} usage documents for EnterpriseCode={enterpriseCode}, ServerCode={serverCode}");
			return transactions;
		}

		public async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delayMilliseconds = 1000)
		{
			for (int attempt = 1; attempt <= maxRetries; attempt++)
			{
				try
				{
					return await action();
				}
				catch (Exception ex) when (IsTransient(ex))
				{
					Logger.LogInformation($"Retry attempt {attempt} for {action.Method.Name} failed: {ex.Message}");

					if (attempt == maxRetries)
					{
						Logger.LogError(ex, $"Max retry attempts reached for {action.Method.Name} ");
						throw ;
					}
					await Task.Delay(delayMilliseconds);
				}
			}
			throw new InvalidOperationException("Unreachable");
		}

		private static bool IsTransient(Exception ex)
		{
			return ex is Elastic.Transport.TransportException ||
			       ex is HttpRequestException ||
			       (ex.InnerException is HttpRequestException);
		}

		private int GetIntParameter(PluginSettings settings, string key, int defaultValue)
		{
			var raw = settings.Parameters.FirstOrDefault(x => x.Name == key)?.Value;
			return int.TryParse(raw, out var result) && result > 0 ? result : defaultValue;
		}

		protected IEnumerable<TimeStampedTransaction> CreateTransactions(params object[] transactionParams)
		{
			var enterpriseCode = transactionParams[0].ToString();
			var serverCode = transactionParams[1].ToString();
			var ref1 = transactionParams[2].ToString();
			var ref2 = transactionParams[3].ToString();
			var ref3 = transactionParams[4].ToString();
			var ref4 = transactionParams[5].ToString();
			var ref5 = transactionParams[6].ToString();
			var occurredUtc = (DateTime)transactionParams[7];
			var maxCores = (double)transactionParams[8];
			var userCount = (int)transactionParams[9];
			var billableCount = (int)(userCount > 0
				? Math.Ceiling(maxCores - (userCount / (double)100.00))
				: Math.Ceiling(maxCores));

			if (billableCount > 0)
			{
				yield return new TimeStampedTransaction(occurredUtc,
					new BillingTransaction()
					{
						BillableCount = billableCount,
						Category = "WGR",
						PriceItemCode = "WGR",
						ClientID = $"{enterpriseCode}???{serverCode}",
						Reference1 = ref1,
						Reference2 = ref2,
						Reference3 = ref3,
						Reference4 = ref4,
						Reference5 = ref5,
						ReportingSource = "MSC",
						ServiceOccuredUTC = occurredUtc,
						Version = 1
					});
			}
		}
		#region Settings

		public string CpuMonitoringIndex { get; set; } = string.Empty;
		public string GatewayNonBilledUsage { get; set; } = string.Empty;
		public int ElasticRetryMaxAttempts { get; set; }
		public int ElasticRetryDelayInSecond { get; set; }
                public int BatchSize { get; set; }
                public Dictionary<string, string[]> DefectiveQueryHashesDictionary { get; set; }  = new();
                #endregion
        }
}
