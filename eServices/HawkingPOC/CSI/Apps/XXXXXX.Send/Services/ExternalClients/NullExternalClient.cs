using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Apps.XXXXXX.Send.Services.ExternalClients
{
    public class NullExternalClient : IExternalClient
    {
        private readonly ILogger logger;

        public NullExternalClient(ILogger<NullExternalClient> logger)
        {
            this.logger = logger;
        }

        public async Task SendAsync(IDictionary<string, StringValues> headers, Stream message, CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            int count, len = 0;
            while ((count = await message.ReadAsync(buffer, 0, buffer.Length)) > 0) { len += count; }

            logger.LogInformation("Nulled message: {TrackingId} StorageSize: {StorageSize} OutputSize: {OutputSize}", 
                headers["X-TrackingId"].ToString(), headers["X-Size"].ToString(), len);
        }
    }
}