using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hawking.Messaging;
using Hawking.Tracking.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Hawking.Apps.CCCCCC.Inbound.Services
{
    public class PollingService : IHostedService
    {
        private readonly IPollingHttpClient pollingHttpClient;
        private readonly IMessageCacheHttpClient messageCacheHttpClient;
        private readonly IMessageEventclient messageEventClient;
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfiguration;
        private readonly double pollingInterval;
        private readonly int pollingLimit;
        private Task pollingTask;

        public PollingService(IPollingHttpClient pollingHttpClient, IMessageCacheHttpClient messageCacheHttpClient, IMessageEventclient messageEventClient, IConfiguration configuration, ILogger<PollingService> logger)
        {
            this.pollingHttpClient = pollingHttpClient;
            this.messageCacheHttpClient = messageCacheHttpClient;
            this.messageEventClient = messageEventClient;
            this.logger = logger;
            serviceConfiguration = configuration.GetSection(nameof(PollingService));
            pollingInterval = double.Parse(serviceConfiguration[$"PollingInterval"]);
            pollingLimit = int.Parse(serviceConfiguration["PollingLimit"]);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Polling started for '{Url}'", pollingHttpClient.HttpClient.BaseAddress);

            pollingTask = PollAsync(cancellationToken);

            await Task.CompletedTask;
        }

        private async Task PollAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < pollingLimit; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var response = await pollingHttpClient.HttpClient.GetAsync(""))
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        logger.LogDebug("No files found.");
                        return;
                    }
                    response.EnsureSuccessStatusCode();

                    var message = new HawkingMessage
                    {
                        Headers = 
                        {
                            { "Sender", serviceConfiguration["Sender"] }, 
                            { "Recipient", serviceConfiguration["Recipient"] }
                        },
                        Body = await response.Content.ReadAsByteArrayAsync()
                    };

                    var request = JsonConvert.SerializeObject(new 
                    {
                        sourceAddress = new ApplicationAddress { ApplicationName = "CCCCCC", ApplicationVersion = "latest" },
                        message = message
                    });

                    logger.LogTrace("Receiving {@Message}", message);

                    await Task.WhenAll(
                        messageEventClient.TrackMessageEventAsync(MessageEventType.Received, message, cancellationToken),
                        messageCacheHttpClient.HttpClient.PostAsync("", new StringContent(request, Encoding.UTF8, "application/json"), cancellationToken)
                    );
                }
                await Task.Delay(TimeSpan.FromMilliseconds(pollingInterval));
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Polling stopped");
            await Task.CompletedTask;
        }
    }
}