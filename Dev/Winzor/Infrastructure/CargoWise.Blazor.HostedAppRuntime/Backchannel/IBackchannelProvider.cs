using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace CargoWise.Blazor.HostedAppRuntime.Backchannel
{
	public interface IBackchannelProvider
	{
		IHubConnector HubConnector { get; }
		bool HybridModeAvailable { get; }

		Task OpenBackchannelAsync(string url);
		void OnOpen(Action<string> onData);

		Action OnConnectFailed { get; set; }

		void OnExit(Action onData);
	}
}
