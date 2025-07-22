using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;

public class Plugin : ElasticSearchPluginBase
{
    public override void UpdateSettings(PluginSettings settings)
    {
        base.UpdateSettings(settings);
        HAProxyIndex = settings.Parameters.FirstOrDefault(x => x.Name.Equals("HAProxyIndex"))?.Value ?? string.Empty;
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
                                new Dictionary<string, CompositeAggregationSource>
                                {
                                    {"client_ip", new () { Terms = new() { Field = "haproxy.client.ip.keyword" } } }
                                }
                            })
                            .Aggregations(sub => sub
                                .Sum("sum_read", sum => sum.Field("haproxy.bytes.read"))
                                .Sum("sum_uploaded", sum => sum.Field("haproxy.bytes_uploaded"))
                                .TopHits("server", th => th
                                    .Size(1)
                                    .Source(sf => sf
                                        .Includes(new[] { new Field("haproxy.server_name") }))
                                )
                            )
                        )
                    )
            ).GetAwaiter().GetResult();

        var ipBuckets = searchResponse.Aggregations?.GetComposite("ip_group")?.Buckets;
        if (ipBuckets == null)
        {
            yield break;
        }

        foreach (var bucket in ipBuckets)
        {
            var ip = bucket.Key.TryGetValue("client_ip", out var ipObj) && ipObj.Value != null
                ? ipObj.Value.ToString() ?? string.Empty
                : string.Empty;
            var bytesRead = bucket.Aggregations.GetSum("sum_read")?.Value ?? 0;
            var bytesUploaded = bucket.Aggregations.GetSum("sum_uploaded")?.Value ?? 0;
            var server = bucket.Aggregations.GetTopHits("server")?.Hits<HaproxyLogEntry>()?.FirstOrDefault()?.ServerName ?? string.Empty;
            var total = bytesRead + bytesUploaded;

            yield return new TimeStampedTransaction(end,
                new BillingTransaction
                {
                    BillableCount = (int)Math.Ceiling(total),
                    Category = "WCR",
                    PriceItemCode = "WCR",
                    ClientID = ip,
                    Reference1 = server,
                    ReportingSource = "MSC",
                    ServiceOccuredUTC = end,
                    Version = 1
                });
        }
    }

    public string HAProxyIndex { get; set; } = string.Empty;
}
