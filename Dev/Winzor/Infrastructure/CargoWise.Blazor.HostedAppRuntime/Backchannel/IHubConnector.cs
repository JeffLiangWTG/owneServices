using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace CargoWise.Blazor.HostedAppRuntime.Backchannel
{
	public interface IHubConnector
	{
		TaskCompletionSource ConnectionStarted { get; }
		HubConnection HubConnection { get; }
		IHubProxy HubProxy { get; }
		Task TryOpenBackchannelAsync(string url);
	}
}
