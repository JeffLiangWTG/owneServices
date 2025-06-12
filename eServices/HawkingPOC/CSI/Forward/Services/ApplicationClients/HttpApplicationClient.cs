using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Forward.Services.ApplicationClients
{
    public class HttpApplicationClient : IApplicationClient
    {
        private readonly HttpClient httpClient;
        private readonly ILogger logger;

        public HttpApplicationClient(HttpClient httpClient, ILogger<HttpApplicationClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public string Uri { get; set; }

        public async Task<IDictionary<string, StringValues>> SendAsync(IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Uri);
            messageHeaders.CopyTo(request.Headers);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response.GetCustomHeaders();
        }
    }
}