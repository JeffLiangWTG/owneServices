using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace CargoWise.Winzor.Telemetry;

public interface IPingService : IDisposable
{
	Task Start();
	void Stop();
}

public class PingService : IPingService, IDisposable
{
	readonly IJSRuntime jsRuntime;
	readonly Stopwatch stopwatch;
	readonly System.Timers.Timer timer;
	readonly ISignalRTelemetry signalRTelemetry;

	public PingService(IJSRuntime jsRuntime, ISignalRTelemetry telemetry, IOptions<TelemetryOptions> options)
	{
		this.jsRuntime = jsRuntime;
		stopwatch = new Stopwatch();
		timer = new System.Timers.Timer(options.Value.MetricExportInterval);
		timer.Elapsed += async (sender, args) => await DoPing();
		signalRTelemetry = telemetry;
	}

	public async Task Start()
	{
		await DoPing();
		timer.Start();
	}

	public void Stop()
	{
		timer.Stop();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	async Task DoPing()
	{
		try
		{
			stopwatch.Restart();
			await jsRuntime.InvokeVoidAsync("Date.now");
			stopwatch.Stop();
			signalRTelemetry.RecordPing(stopwatch.Elapsed.TotalMilliseconds);
		}
		catch (Exception)
		{
			// Exceptions might be thrown in the following cases:
			// Invoking a JS method after the form has been closed and the circuit is no longer available.
			// Invoking a JS method when the circuit is disconnected and then reconnected.
		}
	}

	public void Dispose()
	{
		timer.Dispose();
	}
}
