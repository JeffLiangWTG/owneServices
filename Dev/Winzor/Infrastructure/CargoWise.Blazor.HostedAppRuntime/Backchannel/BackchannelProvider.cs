using System;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using Enterprise.BlazorWinFormsInterop;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CargoWise.Blazor.HostedAppRuntime.Backchannel
{
	public class BackchannelProvider : IBackchannelProvider
	{
		public bool HybridModeAvailable => hubConnector.HubProxy != null;
		public IHubConnector HubConnector => hubConnector;

		public Action OnConnectFailed { get; set; }

		readonly int RetryCount;
		readonly ILogger<BackchannelProvider> logger;
		readonly IHubConnector hubConnector;

		public BackchannelProvider(IOptions<CargoWiseOptions> cargoWiseOptions, ILogger<BackchannelProvider> logger, IHubConnector hubConnector)
		{
			this.RetryCount = cargoWiseOptions.Value.BackChannelRetryCount;
			this.logger = logger;
			this.hubConnector = hubConnector;
		}

		/// <summary>
		/// Opens the backchannel if a URL is provided
		/// </summary>
		/// <param name="url"></param>
		public async Task OpenBackchannelAsync(string url)
		{
			var retryCount = RetryCount;
			if (!string.IsNullOrEmpty(url))
			{
				while (retryCount > 0)
				{
					try
					{
						await hubConnector.TryOpenBackchannelAsync(url);
						return;
					}
					catch (Exception ex) when (ex is HttpRequestException || ex is HttpClientException)
					{
						logger.LogWarning(ex, (NoResString)"Error occured trying to open winzor backchannel");
						retryCount--;
					}
				}
				logger.LogInformation($"Could not establish backchannel connection after retrying {RetryCount} times.");
				OnConnectFailed?.Invoke();
			}
		}

		public void OnOpen(Action<string> onData)
		{
			hubConnector.HubProxy.On(nameof(IBlazorClient.OpenUrlInWinzorMode), onData);
		}

		public void OnExit(Action onData)
		{
			hubConnector.HubProxy.On(nameof(IBlazorClient.ExitWinzorMode), onData);
		}
	}
}
