using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hawking.CSI.Apps.YYYYYY.Outbound.Services.ContentStoreClients
{
    public class HttpContentStoreClient : IContentStoreClient
    {
        private readonly HttpClient httpClient;
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfiguration;

        public HttpContentStoreClient(HttpClient httpClient, IConfiguration configuration, ILogger<HttpContentStoreClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            serviceConfiguration = configuration.GetSection(nameof(HttpContentStoreClient));
            httpClient.BaseAddress = new Uri(serviceConfiguration["Uri"]);
        }

        public async Task<Stream> ReadContentAsync(string reference, CancellationToken cancellationToken)
        {
            return await httpClient.GetStreamAsync(reference);
        }

        public async Task<string> WriteContentAsync(IDictionary<string, StringValues> headers, Stream message, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            headers.CopyTo(request.Headers);
            request.Content = new PushStreamContent(async (body, HttpContent, context) =>
            {
                await message.CopyToAsync(body);
                body.Close();
            });

            var response = await httpClient.SendAsync(request, cancellationToken);
            return response.Headers.GetValues("X-Content-Store-Reference").FirstOrDefault();
        }
    }
}