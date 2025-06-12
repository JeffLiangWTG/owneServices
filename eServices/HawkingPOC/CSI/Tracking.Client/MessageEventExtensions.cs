using System;
using System.Net.Http;
using Hawking.CSI.Tracking.Client.MessageEventClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Hawking.CSI.Tracking.Client
{
    public static class MessageEventExtensions
    {
        public static IServiceCollection AddMessageEventClient(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var logger = serviceProvider.GetService<ILogger<HttpMessageEventClient>>();
            
            var trackingUri = configuration[$"{nameof(HttpMessageEventClient)}:Uri"];
            var timeout = TimeSpan.FromSeconds(int.TryParse(configuration[$"{nameof(HttpMessageEventClient)}:Timeout"], out int seconds) ? seconds : 30);

            if (!Uri.TryCreate(trackingUri, UriKind.Absolute, out Uri hostUri))
            {
                logger.LogError("Configuration missing or invalid for Event Tracking URI");
                throw new InvalidOperationException("Configuration missing or invalid for Event Tracking URI");
            }

            logger.LogInformation("Registering message event client for URI {Uri} with timeout {Timeout}", hostUri, timeout);

            services.AddTransient<IMessageEventClient, HttpMessageEventClient>();
            services.AddHttpClient<IMessageEventClient, HttpMessageEventClient>(client => client.BaseAddress = hostUri)
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(timeout))
                .AddTransientHttpErrorPolicy(config => config.RetryForeverAsync());

            return services;
        }
    }
}
