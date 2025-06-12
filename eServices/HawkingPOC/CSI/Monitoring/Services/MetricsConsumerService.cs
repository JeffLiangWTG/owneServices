using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Monitoring.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Hawking.CSI.Monitoring.Services
{
    public class MetricsConsumerService : IHostedService
    {
        private readonly IEventStreamClient eventStreamClient;
        private readonly IMetricsCacheClient metricsCacheClient;
        private readonly IMetricsStoreClient metricsStoreClient;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public MetricsConsumerService(
            IEventStreamClient eventStreamClient,
            IMetricsCacheClient metricsCacheClient,
            IMetricsStoreClient metricsStoreClient,
            IConfiguration configuration, 
            ILogger<MetricsConsumerService> logger)
        {
            this.eventStreamClient = eventStreamClient;
            this.metricsCacheClient = metricsCacheClient;
            this.metricsStoreClient = metricsStoreClient;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {ServiceName} on host {HostName}", nameof(MetricsConsumerService), Environment.MachineName);
            await eventStreamClient.SubscribeAsync(ConsumerAction);
        }

        public async void ConsumerAction(object eventSender, ConsumerEventArgs args)
        {
            try
            {
                var rawMsg = Encoding.UTF8.GetString(args.Data);
                var message = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(Encoding.UTF8.GetString(args.Data))
                    .ToDictionary(h => h.Key, h => new StringValues(h.Value));
                var trackingId = Guid.Parse(message["X-TrackingId"]);

                var tranEvent = new TransactionEvent
                {
                    Offset = args.Sequence,
                    TrackingId = trackingId,
                    Type = message["X-Event-Type"],
                    Time = DateTime.Parse(message["X-Event-Time"]),
                };

                if (message.TryGetValue("X-Sender", out StringValues sender))
                    tranEvent.Sender = sender;
                if (message.TryGetValue("X-Recipient", out StringValues recipient))
                    tranEvent.Recipient = recipient;
                if (message.TryGetValue("X-Content-Store-Reference", out StringValues storageReference))
                    tranEvent.StorageReference = storageReference;
                if (message.TryGetValue("X-Size", out StringValues sizeText))
                    if (long.TryParse(sizeText, out long size))
                        tranEvent.Size = size;

                logger.LogTrace("TransactionEvent: {@TranEvent}", tranEvent);

                await metricsCacheClient.AddAsync(tranEvent);
                
                args.Ack();

                if (tranEvent.Type == "Sent" || tranEvent.Type == "Acknowledged")
                {
                    var transactionMetrics = await metricsCacheClient.GetAsync(trackingId);
                    await metricsStoreClient.StoreMetrics(transactionMetrics);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error recieving message: {@Message}", args);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping {ServiceName} on host {HostName}", nameof(MetricsConsumerService), Environment.MachineName);
            await Task.CompletedTask;
        }
    }
}