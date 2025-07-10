using System;
using System.Threading.Tasks;
using CargoWise.Winzor.Telemetry;
using Microsoft.AspNetCore.SignalR;

namespace CargoWise.Winzor.AppServer.Telemetry;

public class TelemetryHubFilter : IHubFilter
{
	readonly ISignalRTelemetry signalRTelemetry;

	public TelemetryHubFilter(ISignalRTelemetry signalRTelemetry)
	{
		this.signalRTelemetry = signalRTelemetry;
	}

	public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
	{
		// for incoming messages we only care BeginInvokeDotNetFromJS that represents a interaction where JavaScript calls a .NET method
		if (invocationContext.HubMethodName != "BeginInvokeDotNetFromJS")
		{
			return await next(invocationContext);
		}

#pragma warning disable CW1161 // Res.GetString Analyzer
		string dotNetMethodName = null;
#pragma warning restore CW1161 // Res.GetString Analyzer
		if (invocationContext.HubMethodArguments.Count > 2)
		{
			dotNetMethodName = invocationContext.HubMethodArguments[2].ToString();
		}
		using var activity = signalRTelemetry.ActivitySource.StartActivity($"BeginInvokeDotNetFromJS.{dotNetMethodName}");
		return await next(invocationContext);
	}
}
