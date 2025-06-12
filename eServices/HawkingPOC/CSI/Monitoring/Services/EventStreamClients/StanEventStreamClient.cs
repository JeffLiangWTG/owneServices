using System;
using System.Threading.Tasks;
using Hawking.CSI.Monitoring.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using STAN.Client;

namespace Hawking.CSI.Monitoring.Services.EventStreamClients
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
        private readonly RetryPolicy subscribeRetryPolicy;
        private IStanConnection stanConnection;
        private PolicyResult<IStanSubscription> subscription;

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
            subscribeRetryPolicy = Policy.Handle<Exception>().RetryForeverAsync(async ex =>
            {
                logger.LogWarning("Error subscribing to NATS Streaming subject '{Subject}' - {Type}: {Message}. Recreating connection", subject, ex.GetType(), ex.Message);
                stanConnection = await ConnectAsync();
            });
            stanConnection = ConnectAsync().Result;
        }

        public async Task SubscribeAsync(EventHandler<ConsumerEventArgs> consumerAction)
        {
            var subOpts = StanSubscriptionOptions.GetDefaultOptions();
            subOpts.DurableName = serviceConfig["Subscription"];
            subOpts.ManualAcks = true;

            logger.LogInformation("Subscribing to NATS Streaming with options {@SubOpts}", subOpts);

            var subscriber = (EventHandler<StanMsgHandlerArgs>)((sender, args) =>
            {
                var consumerArgs = new ConsumerEventArgs(args.Message.TimeStamp, args.Message.Sequence,
                    args.Message.Subject, args.Message.Data, () =>
                    {
                        args.Message.Ack();
                    });
                consumerAction(sender, consumerArgs);
            });

            subscription = await subscribeRetryPolicy.ExecuteAndCaptureAsync(async () =>
                await Task.Run(() => stanConnection.Subscribe(serviceConfig["Subject"], subOpts, subscriber)));
            
            logger.LogInformation("Succesfully subscribed to NATS Streaming {@Subscription}", subscription);
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