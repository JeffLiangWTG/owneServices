using System;
using System.Threading.Tasks;
using Hawking.CSI.Monitoring.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;

namespace Hawking.CSI.Monitoring.Services.MetricsStoreClients
{
    public class ElasticsearchMetricsStoreClient : IMetricsStoreClient
    {
        private readonly ILogger<ElasticsearchMetricsStoreClient> logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly ElasticClient client;

        public ElasticsearchMetricsStoreClient(IConfiguration configuration, ILogger<ElasticsearchMetricsStoreClient> logger)
        {
            this.logger = logger;
            serviceConfig = configuration.GetSection(nameof(ElasticsearchMetricsStoreClient));
            var elasticUri = serviceConfig["HostUri"];
            logger.LogInformation("Connecting to Elasticsearch server at {elaticUri}", elasticUri);

            var node = new Uri(elasticUri);
            var settings = new ConnectionSettings(node);
            client = new ElasticClient(settings);
        }

        public async Task StoreMetrics(TransactionMetrics metrics)
        {
            await client.IndexAsync(metrics, idx => idx.Index(serviceConfig["Index"])
                .Id(metrics.TrackingId));
        }
    }
}
