using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using STAN.Client;

namespace Hawking.CSI.Tracking.Services.EventStreamClients
{
    public class StanEventStreamClient : IEventStreamClient
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly string hostName;
        private readonly string natsUri;
        private readonly string clusterId;
        private readonly string subject;
        private readonly RetryPolicy connectionRetryPolicy;
        private readonly RetryPolicy publishRetryPolicy;
        private IStanConnection stanConnection;

        public StanEventStreamClient(IConfiguration configuration, ILogger<StanEventStreamClient> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            serviceConfig = configuration.GetSection(nameof(StanEventStreamClient));
            hostName = Environment.MachineName;
            logger.LogInformation("Starting service '{ServiceName}' on host '{HostName}'", nameof(StanEventStreamClient), hostName);
            natsUri = serviceConfig["HostUri"];
            clusterId = serviceConfig["ClusterId"];
            subject = serviceConfig["Subject"];
            connectionRetryPolicy = Policy.Handle<Exception>().WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(1),
                (ex, t) => logger.LogWarning("Error connecting to NATS Streaming server - {Type}: {Message}", ex.GetType(), ex.Message));
            publishRetryPolicy = Policy.Handle<Exception>().RetryForeverAsync(async ex =>
            {
                logger.LogWarning("Error publishing to NATS Streaming subject '{Subject}' - {Type}: {Message}. Recreating connection", subject, ex.GetType(), ex.Message);
                stanConnection = await ConnectAsync();
            });
            stanConnection = ConnectAsync().Result;
        }

        public async Task PublishEventAsync(IDictionary<string, StringValues> messageHeaders)
        {
            logger.LogDebug("Publishing tracking event {@Event}", messageHeaders);
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageHeaders));
            await publishRetryPolicy.ExecuteAsync(() => stanConnection.PublishAsync(subject, body));
        }

        private async Task<IStanConnection> ConnectAsync()
        {
            logger.LogInformation("Connecting to NATS Streaming host {URL} and cluster '{ClusterId}' as client ID '{ClientID}'", natsUri, clusterId, hostName);
            var stanOpts = StanOptions.GetDefaultOptions();
            stanOpts.NatsURL = natsUri;
            var connectionFactory = new StanConnectionFactory();
            var execResult = await connectionRetryPolicy.ExecuteAndCaptureAsync(() =>
                Task.Run(() => connectionFactory.CreateConnection(clusterId, hostName, stanOpts))
            );
            logger.LogInformation("Successfully connected to NATS Streaming host {URL}", natsUri);
            return await Task.FromResult(execResult.Result);
        }
    }
}