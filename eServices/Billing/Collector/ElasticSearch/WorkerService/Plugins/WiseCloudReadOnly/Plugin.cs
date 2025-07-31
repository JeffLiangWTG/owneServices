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

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly
{
	public class Plugin : ElasticSearchPluginBase
	{
		public override void UpdateSettings(PluginSettings settings)
		{
			base.UpdateSettings(settings);
			HAProxyIndex = settings.Parameters.FirstOrDefault(x => x.Name.Equals("HAProxyIndex"))?.Value ?? string.Empty;
			ReferenceFilePath = settings.Parameters.FirstOrDefault(x => x.Name.Equals("ReferenceFilePath"))?.Value ?? string.Empty;
			ElasticRetryMaxAttempts = GetIntParameter(settings, "ElasticRetryMaxAttempts", 1440);
			ElasticRetryDelayInSecond = GetIntParameter(settings, "ElasticRetryDelayInSecond", 60);
			BatchSize = GetIntParameter(settings, "BatchSize", 20);
		}

		public IEnumerable<TimeStampedTransaction> GetTransactionInPage(DateTime start, DateTime end)
		{
			var mapping = ReferenceFileProcessor.ProcessReferenceFile(ReferenceFilePath, Logger);
			IReadOnlyDictionary<string, FieldValue>? afterKey = null;

			while (true)
			{
				var searchResponse = Client.SearchAsync<HaproxyLogEntry>(s => s
					.Index(HAProxyIndex)
					.Size(0)
					.Query(q => q
						.Bool(b => b
							.Filter(f => f
								.Range(r => r
									.DateRange(dr => dr
										.Field("@timestamp")
										.Gte(start)
										.Lte(end)
									)
								)
							)
						)
					)
					.Aggregations(aggs => aggs
						.Add("ip_group", c => c
							.Composite(comp =>
							{
								comp.Size(BatchSize)
									.Sources(new List<IDictionary<string, CompositeAggregationSource>>
									{
										new Dictionary<string, CompositeAggregationSource> { { "client_ip", new() { Terms = new() { Field = "haproxy.client.ip" } } } },
										new Dictionary<string, CompositeAggregationSource> { { "backend_name", new() { Terms = new() { Field = "haproxy.backend.name" } } } },
										new Dictionary<string, CompositeAggregationSource> { { "server_port", new() { Terms = new() { Field = "haproxy.server_port" } } } }
									});

								if (afterKey != null)
								{
									comp.After(f =>
									{
										foreach (var kvp in afterKey)
										{
											f.Add(kvp.Key, kvp.Value.ToString());
										}
										return f;
									});
								}
							})
							.Aggregations(sub => sub
								.Add("sum_read", s => s.Sum(sum => sum.Field("haproxy.bytes.read")))
								.Add("sum_uploaded", s => s.Sum(sum => sum.Field("haproxy.bytes.uploaded")))
								.Add("min_timestamp", s => s.Min(m => m.Field("@timestamp")))
							)
						)
					)
				).GetAwaiter().GetResult();

				var buckets = searchResponse.Aggregations.GetComposite("ip_group")?.Buckets;
				if (buckets == null || buckets.Count == 0)
					break;

				foreach (var bucket in buckets)
				{
					var clientIp = bucket.Key["client_ip"].ToString();
					var backendName = bucket.Key["backend_name"].ToString();
					var serverPort = bucket.Key["server_port"].ToString();
					var sumRead = bucket.Aggregations.GetSum("sum_read")?.Value ?? 0;
					var sumUploaded = bucket.Aggregations.GetSum("sum_uploaded")?.Value ?? 0;
					var minTimestamp = bucket.Aggregations.GetMin("min_timestamp")?.ValueAsString;

					var total = sumRead + sumUploaded;
					Logger.LogInformation($"Processing system {clientIp}: sumRead={sumRead}, sumUploaded={sumUploaded}, total={total},");

					if (mapping.TryGetValue(clientIp, out var codes) && codes.Count > 0)
					{
						var clientCode = codes[0].Insert(3, "???");
						var billable = (int)Math.Round(total / 1_000_000d, MidpointRounding.AwayFromZero);
						if (billable > 0)
						{
							yield return new TimeStampedTransaction(end, new BillingTransaction
							{
								BillableCount = billable,
								Category = "HOS",
								PriceItemCode = "#HA",
								ClientID = clientCode,
								Reference1 = clientIp,
								Reference2 = backendName,
								Reference3 = minTimestamp,
								Reference4 = serverPort,
								ReportingSource = "MSC",
								ServiceOccuredUTC = end,
								Version = 1
							});
						}
					}
				}

				afterKey = searchResponse.Aggregations.GetComposite("ip_group")?.AfterKey;
			}

			var tasks = new List<Task<(string EnterpriseCode, string ServerCode, IEnumerable<TimeStampedTransaction> Transactions)>>();
			foreach (var remaining in tasks)
			{
				var result = remaining.Result;
				foreach (var tx in result.Transactions)
				{
					yield return tx;
				}
			}
		}

		public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
		{

			var searchResponse = Client.SearchAsync<HaproxyLogEntry>(s => s
				.Index(HAProxyIndex)
				.Size(0)
				.Query(q => q
					.Bool(b => b
						.Filter(f => f
							.Range(r => r
								.DateRange(dr => dr
									.Field("@timestamp")
									.Gte(start)
									.Lte(end)
								)))))
				.Aggregations(aggs => aggs
					.Add("ip_group", c => c
						.Composite(comp => comp
							.Size(10000)
							.Sources(new List<IDictionary<string, CompositeAggregationSource>>
							{
								new Dictionary<string, CompositeAggregationSource> { { "client_ip", new () { Terms = new() { Field = "haproxy.client.ip" } } } },
								new Dictionary<string, CompositeAggregationSource> { { "backend_name", new () { Terms = new() { Field = "haproxy.backend.name" } } } },
								new Dictionary<string, CompositeAggregationSource> { { "server_port", new () { Terms = new() { Field = "haproxy.server_port" } } } }
							}))
						.Aggregations(sub => sub
							.Add("sum_read", s => s.Sum(sum => sum.Field("haproxy.bytes.read")))
							.Add("sum_uploaded", s => s.Sum(sum => sum.Field("haproxy.bytes.uploaded")))
							.Add("min_timestamp", s => s.Min(m => m.Field("@timestamp")))
						)
					)
				)
			).GetAwaiter().GetResult();

			var mapping = ReferenceFileProcessor.ProcessReferenceFile(ReferenceFilePath, Logger);
			var buckets = searchResponse.Aggregations.GetComposite("ip_group")?.Buckets;
			if (buckets != null)
			{
				foreach (var bucket in buckets)
				{
					var clientIp = bucket.Key["client_ip"].ToString();
					var backendName = bucket.Key["backend_name"].ToString();
					var serverPort = bucket.Key["server_port"].ToString();
					var sumRead = bucket.Aggregations.GetSum("sum_read")?.Value ?? 0;
					var sumUploaded = bucket.Aggregations.GetSum("sum_uploaded")?.Value ?? 0;
					var minTimestamp = bucket.Aggregations.GetMin("min_timestamp")?.ValueAsString;

					var total = sumRead + sumUploaded;
					Logger.LogInformation($"Processing system {clientIp}: sumRead={sumRead}, sumUploaded={sumUploaded},  total={total},");

					if (mapping.TryGetValue(clientIp, out var codes) && codes.Count > 0)
					{
						var clientCode = codes[0].Insert(3, "???");
						var billable = (int)Math.Round(total / 1_000_000d, MidpointRounding.AwayFromZero);
						if (billable > 0)
						{
							yield return new TimeStampedTransaction(end, new BillingTransaction
							{
								BillableCount = billable,
								Category = "HOS",
								PriceItemCode = "#HA",
								ClientID = clientCode,
								Reference1 = clientIp,
								Reference2 = backendName,
								Reference3 = minTimestamp,
								Reference4 = serverPort,
								ReportingSource = "MSC",
								ServiceOccuredUTC = end,
								Version = 1
							});
						}
					}
				}
			}

			var tasks = new List<Task<(string EnterpriseCode, string ServerCode, IEnumerable<TimeStampedTransaction> Transactions)>>();
			int currentIndex = 0;

			foreach (var remaining in tasks)
			{
				var result = remaining.Result;
				foreach (var tx in result.Transactions)
				{
					yield return tx;
				}
			}
		}

		#region Settings

		public string HAProxyIndex { get; set; } = string.Empty;
		public string ReferenceFilePath { get; set; } = string.Empty;
		public int ElasticRetryMaxAttempts { get; set; }
		public int ElasticRetryDelayInSecond { get; set; }
		public int BatchSize { get; set; }
		#endregion
	}
}
