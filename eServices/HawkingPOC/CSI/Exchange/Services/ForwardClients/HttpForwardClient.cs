using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Exchange.ForwardClients
{
    public class HttpForwardClient : IForwardClient
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection serviceConfig;

        public HttpForwardClient(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            serviceConfig = configuration.GetSection(nameof(HttpForwardClient));
            httpClient.BaseAddress = new Uri(serviceConfig["Uri"]);
        }

        public async Task SendAsync(IDictionary<string, StringValues> headers, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "");
            headers.CopyTo(request.Headers);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}