using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.BlazorWinFormsInterop;
using Microsoft.AspNet.SignalR.Client;

namespace CargoWise.Blazor.HostedAppRuntime.Backchannel
{
	public class HubConnector : IHubConnector
	{
		public HubConnection HubConnection { get; private set; }
		public IHubProxy HubProxy { get; private set; }

		public TaskCompletionSource ConnectionStarted { get; } = new ();

		public async Task TryOpenBackchannelAsync(string url)
		{
			HubConnection = new HubConnection(url);
			HubProxy = HubConnection.CreateHubProxy(nameof(BlazorWinFormsInteropHub));
			await HubConnection.Start();
			ConnectionStarted.SetResult();
		}
	}
}
