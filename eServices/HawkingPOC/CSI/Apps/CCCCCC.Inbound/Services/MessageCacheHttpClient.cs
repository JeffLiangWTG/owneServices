using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Hawking.Apps.CCCCCC.Inbound.Services
{
    public class MessageCacheHttpClient : IMessageCacheHttpClient
    {
        private readonly IConfiguration configuration;

        public MessageCacheHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            HttpClient = httpClient;
            this.configuration = configuration;
            HttpClient.BaseAddress = new Uri(configuration[$"{nameof(MessageCacheHttpClient)}:Url"]);
        }

        public HttpClient HttpClient { get; }
    }

    public interface IMessageCacheHttpClient
    {
        HttpClient HttpClient { get; }
    }
}