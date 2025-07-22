using System.Diagnostics;
using System.Collections.Generic;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common;
using CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.MSearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Newtonsoft.Json;
using SourceFilter = Elastic.Clients.Elasticsearch.Core.Search.SourceFilter;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;

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

        if (!string.IsNullOrEmpty(ReferenceFilePath))
        {
            IpMappings = IPReferenceFileProcessor.ProcessReferenceFile(ReferenceFilePath, Logger);
        }
    }


    public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
    {
        var searchResponse = Client.SearchAsync<HaproxyLogEntry>(s => s
            .Index(HAProxyIndex)
            .Size(10000)
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
                            new Dictionary<string, CompositeAggregationSource>
                            {
                                {"client_ip", new () { Terms = new() { Field = "haproxy.client.ip" } } }
                            }
                        })
                    .Aggregations(sub => sub
                        .Add("sum_read", s => s
                            .Sum(sum => sum
                                .Script(script => script
                                    .Source("doc['haproxy.bytes.read'].size() != 0 ? doc['haproxy.bytes.read'].value : 0")
                                )
                            )
                        )
                        .Add("sum_uploaded", s => s
                            .Sum(sum => sum
                                .Script(script => script
                                    .Source("doc['haproxy.bytes_uploaded'].size() != 0 ? Integer.parseInt(doc['haproxy.bytes_uploaded'].value.toString()) : 0")
                                )
                            )
                        )
                    )
                )
            )
        ).GetAwaiter().GetResult();

        var buckets = searchResponse.Aggregations.GetComposite("ip_group")?.Buckets;
        if (buckets != null)
        {
            foreach (var bucket in buckets)
            {
                var clientIp = bucket.Key["client_ip"].ToString();
                var sumRead = bucket.Aggregations.GetSum("sum_read")?.Value ?? 0;
                var sumUploaded = bucket.Aggregations.GetSum("sum_uploaded")?.Value ?? 0;

                var total = sumRead + sumUploaded;

                if (IpMappings.TryGetValue(clientIp, out var codes) && codes.Count > 0)
                {
                    var code = codes[0];
                    var enterpriseCode = code.Substring(0, 3);
                    var serverCode = code.Substring(3);
                    var occurredUtc = start;

                    yield return new TimeStampedTransaction(occurredUtc,
                        new BillingTransaction
                        {
                            BillableCount = total,
                            Category = "WGR",
                            PriceItemCode = "WGR",
                            ClientID = $"{enterpriseCode}???{serverCode}",
                            Reference1 = clientIp,
                            ReportingSource = "MSC",
                            ServiceOccuredUTC = occurredUtc,
                            Version = 1
                        });
                }
                else
                {
                    Logger.LogWarning($"IP {clientIp} not found in reference file");
                }
            }
        }
    }

    #region Settings

    public string HAProxyIndex { get; set; } = string.Empty;
    public string ReferenceFilePath { get; set; } = string.Empty;
    internal Dictionary<string, List<string>> IpMappings { get; private set; } = new();
    public int ElasticRetryMaxAttempts { get; set; }
    public int ElasticRetryDelayInSecond { get; set; }
    public int BatchSize { get; set; }
    #endregion

    private int GetIntParameter(PluginSettings settings, string key, int defaultValue)
    {
        var raw = settings.Parameters.FirstOrDefault(x => x.Name == key)?.Value;
        return int.TryParse(raw, out var result) && result > 0 ? result : defaultValue;
    }
}
