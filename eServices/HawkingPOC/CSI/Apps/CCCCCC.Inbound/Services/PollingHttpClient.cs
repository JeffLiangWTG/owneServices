using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hawking.Apps.CCCCCC.Inbound.Services
{
    public class PollingHttpClient : IPollingHttpClient
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public PollingHttpClient(HttpClient httpClient, IConfiguration configuration, ILogger<PollingHttpClient> logger)
        {
            HttpClient = httpClient;
            this.configuration = configuration;
            this.logger = logger;
            HttpClient.BaseAddress = new Uri(configuration[$"{nameof(PollingHttpClient)}:Url"]);
         }

        public HttpClient HttpClient { get; }
    }

    public interface IPollingHttpClient
    {
        HttpClient HttpClient { get; }
    }
}