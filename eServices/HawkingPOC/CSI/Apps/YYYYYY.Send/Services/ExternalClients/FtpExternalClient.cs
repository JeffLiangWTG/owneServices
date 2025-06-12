using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Plugins.FTP;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.Retry;

namespace Hawking.CSI.Apps.YYYYYY.Send.Services.ExternalClients
{
    public class FtpExternalClient : IExternalClient
    {
        private readonly ConcurrentStack<FtpClient> ftpClients = new ConcurrentStack<FtpClient>();
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly FtpOptions ftpOptions;
        private readonly string folder;
        private readonly RetryPolicy retryPolicy;

        public FtpExternalClient(IConfiguration configuration, ILogger<FtpExternalClient> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            serviceConfig = configuration.GetSection(nameof(FtpExternalClient));
            var uri = new UriInfo(serviceConfig["Uri"]);
            ftpOptions = new FtpOptions();
            ftpOptions.User = uri.User;
            ftpOptions.Password = uri.Password;
            ftpOptions.Server = uri.Host;
            if (uri.Port.HasValue)
                ftpOptions.Port = uri.Port.Value;
            folder = uri.Path;
            retryPolicy = Policy.Handle<MessagingException>().WaitAndRetryForeverAsync(
                (i, ex, cd) => 
                {
                    logger.LogWarning("FTP error for file {FileName} - {Error}", cd["FileName"], ex.Message);
                    return TimeSpan.FromSeconds(5);
                }, (ex, ts, cd) => Task.CompletedTask);
        }

        public async Task SendAsync(IDictionary<string, StringValues> headers, Stream message, CancellationToken cancellationToken)
        {
            var contextData = new Context();
            contextData.Add("FileName", $"{DateTime.Now.ToString("yyyyMMdd'-'HHmmssfffffff")}-{headers["X-Sender"]}-{headers["X-Recipient"]}-{headers["X-TrackingId"]}.json");
            await retryPolicy.ExecuteAsync(async (cd, ct) =>
            {
                string fileName = (string)cd["FileName"];
                FtpClient ftpClient;
                if (!ftpClients.TryPop(out ftpClient))
                {
                    ftpClient = new FtpClient();
                    await ftpClient.OpenAsync(ftpOptions, ct);
                }
                cancellationToken.ThrowIfCancellationRequested();

                await ftpClient.PutFileAsync($"{folder}/{fileName}", message, ct);
                ftpClients.Push(ftpClient);
            },
            contextData, cancellationToken);
        }
    }
}