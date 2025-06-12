using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Tracking.Client.MessageEventClients
{
    public class HttpMessageEventClient : IMessageEventClient
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public HttpMessageEventClient(HttpClient httpClient, IConfiguration configuration, ILogger<HttpMessageEventClient> logger)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task TrackMessageEventAsync(MessageEventType eventType, IDictionary<string, StringValues> messageHeaders, 
            CancellationToken cancellationToken)
        {
            await TrackMessageEventInternalAsync(eventType, null, messageHeaders, cancellationToken);
        }

        public async Task TrackMessageEventAsync(MessageEventType eventType, IDictionary<string, StringValues> eventHeaders, 
            IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken)
        {
            await TrackMessageEventInternalAsync(eventType, eventHeaders, messageHeaders, cancellationToken);
        }

        internal async Task TrackMessageEventInternalAsync(MessageEventType eventType, IDictionary<string, StringValues> eventHeaders, 
            IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "");
                request.Headers.Add("X-Event-Type", eventType.ToString());
                request.Headers.Add("X-Event-Source", configuration["HOSTNAME"]);
                request.Headers.Add("X-Event-Time", DateTime.UtcNow.ToString("O"));
                if (eventHeaders != null) eventHeaders.CopyTo(request.Headers);
                messageHeaders.CopyTo(request.Headers);
                
                (await httpClient.SendAsync(request, cancellationToken)).EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error publishing tracking event");
                throw new MessagingException("Error publishing tracking event. See InnerException for details.", ex);
            }
        }
    }
}
