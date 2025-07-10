using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CargoWise.Blazor.SessionBroker
{
	public class VersionBrokerRegistration
	{
		readonly IConfiguration config;
		readonly IServer server;
		readonly IHttpClientFactory httpClientFactory;
		readonly ILogger<VersionBrokerRegistration> logger;

		public VersionBrokerRegistration(IConfiguration config, IServer server, IHttpClientFactory httpClientFactory, ILogger<VersionBrokerRegistration> logger)
		{
			this.config = config;
			this.server = server;
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public async Task RegisterAsync()
		{
			var registrationUrl = config.GetValue<string>("VersionBrokerRegistrationCallback");
			if (registrationUrl != null)
			{
				try
				{
					if (registrationUrl.Length == 0)
					{
						throw new ArgumentException("VersionBrokerRegistrationCallback url should not be empty");
					}
					var address = server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault().Replace("[::]", "localhost", StringComparison.OrdinalIgnoreCase);
					using var httpClient = httpClientFactory.CreateClient();
					using var request = new HttpRequestMessage(HttpMethod.Post, registrationUrl);
					request.Content = new StringContent(address);
					logger.LogInformation($"Registering with Version Broker at {registrationUrl}");
					var response = await httpClient.SendAsync(request);
					response.EnsureSuccessStatusCode();
					logger.LogInformation($"Registered with Version Broker");
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Version broker registration failed, exiting...");
					throw;
				}
			}
			else
			{
				logger.LogInformation("Not registering with Version Broker as registrationUrl is null");
			}
		}
	}
}
