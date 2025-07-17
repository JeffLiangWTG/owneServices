using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Tests;

[TestFixture]
public class WiseCloudReadOnlyPluginTest
{
    [Test]
    public void TestGetTransaction()
    {
        var prodsettings = new PluginSettings
        {
            Parameters = new PluginParameter[]
            {
                new PluginParameter("ElasticEndpoint", "https://elastic.apac-prod-1.wtg.zone"),
                new PluginParameter("ElasticApiKey", "bZYGaEEythS7mCsj4EoC6ivpn24fMoD87l/3dq85L7/qlXKcs84R1UslZFJcYtZ8hPnUvvE2834ZKLpHx98LICtRw2ELux1iVUPrF4O8GQaRmKtdKHtGh9oYUS9hdeUYrU+0swl4NUguOQ2tLP0lBekILmwHrYZxrn7X3k6T+KI="),
                new PluginParameter("HaproxyLogIndex", "logs-haproxy.logfile-wtg")
            }
        };

        var plugin = new Plugin();
        plugin.UpdateSettings(prodsettings);
        var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        plugin.SetLoggerFactory(new LoggerFactory().AddSerilog(logger));
        var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 1, 1, 1, 0, 0, DateTimeKind.Utc);
        var timeStampedTransactions = plugin.GetTransactions(start, end).ToList();
        Assert.IsEmpty(timeStampedTransactions);
    }
}
