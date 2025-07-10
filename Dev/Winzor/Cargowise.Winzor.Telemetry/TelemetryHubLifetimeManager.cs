using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CargoWise.Winzor.Telemetry;
public class TelemetryHubLifetimeManager<THub> : DefaultHubLifetimeManager<THub> where THub : Hub
{
	readonly IWinzorTelemetry telemetry;
	public TelemetryHubLifetimeManager(IWinzorTelemetry telemetry, ILogger<DefaultHubLifetimeManager<THub>> logger) : base(logger)
	{
		this.telemetry = telemetry;
	}

	public override async Task SendConnectionAsync(string connectionId, string methodName, object?[] args, CancellationToken cancellationToken = default)
	{
		// for outgoing messages, we only want to track the JS.BeginInvokeJS that allow .NET code to call JavaScript functions
		// and JS.RenderBatch messages that contains operations which are used to render Blazor components
		if (methodName != "JS.BeginInvokeJS" && methodName != "JS.RenderBatch")
		{
			await base.SendConnectionAsync(connectionId, methodName, args, cancellationToken);
			return;
		}

		using var activity = telemetry.InternalActivity?.StartActivity(methodName);
		var messageSize = methodName.Length;
		foreach (var arg in args)
		{
			messageSize += arg switch
			{
				string str => Encoding.UTF8.GetByteCount(str),
				ArraySegment<byte> array => array.Count,
				_ => 1, //default for bool and int because they have a small influence on message size
			};
		}
		activity?.AddTag("MessageSize", messageSize);

		await base.SendConnectionAsync(connectionId, methodName, args, cancellationToken);
	}
}
