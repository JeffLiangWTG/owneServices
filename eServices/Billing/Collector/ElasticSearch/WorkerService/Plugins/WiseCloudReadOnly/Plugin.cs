using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;

public class Plugin : ElasticSearchPluginBase
{
    public override void UpdateSettings(PluginSettings settings)
    {
        base.UpdateSettings(settings);
        // TODO: parse WiseCloudReadOnly specific settings
    }

    public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
    {
        // TODO: implement logic to retrieve transactions from WiseCloud
        yield break;
    }
}
