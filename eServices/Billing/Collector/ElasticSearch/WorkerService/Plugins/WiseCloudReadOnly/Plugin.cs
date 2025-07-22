using System;
using System.Linq;
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
        var response = Client.SearchAsync<HaproxyLogEntry>(s => s
                .Index(HAProxyIndex)
                .Size(1000)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Range(r => r
                                .DateRange(dr => dr
                                    .Field("@timestamp")
                                    .Gte(start)
                                    .Lte(end)
                                )))))
                .Source(sf => sf
                    .Includes(new[]
                    {
                        new Field("haproxy.client.ip"),
                        new Field("haproxy.bytes.read"),
                        new Field("haproxy.bytes_uploaded"),
                        new Field("haproxy.server_name")
                    }))
            ).GetAwaiter().GetResult();

        var grouped = response.Documents
            .GroupBy(d => d.ClientIp);

        foreach (var group in grouped)
        {
            double total = group.Sum(g => g.BytesRead + g.BytesUploaded);
            var first = group.First();
            yield return new TimeStampedTransaction(end,
                new BillingTransaction
                {
                    BillableCount = (int)Math.Ceiling(total),
                    Category = "WCR",
                    PriceItemCode = "WCR",
                    ClientID = group.Key,
                    Reference1 = first.ServerName,
                    ReportingSource = "MSC",
                    ServiceOccuredUTC = end,
                    Version = 1
                });
        }
    }

    public string HAProxyIndex { get; set; } = string.Empty;
}
