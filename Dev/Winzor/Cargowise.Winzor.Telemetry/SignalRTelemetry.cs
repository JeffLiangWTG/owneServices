using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CargoWise.Winzor.Telemetry;

public interface ISignalRTelemetry : IDisposable
{
	void RecordPing(double? value);

	ActivitySource ActivitySource { get; }
}

public class SignalRTelemetry : ISignalRTelemetry
{
	readonly Meter meter;

	public const string SourceName = "CargoWise.Winzor.SignalR";
	public ActivitySource ActivitySource { get; } = new ActivitySource(SourceName);

	double? invokeVoidAsyncTime;

	public SignalRTelemetry()
	{
		meter = new Meter(SourceName);
		meter.CreateObservableGauge("signalr.ping", () =>
		{
			if (!invokeVoidAsyncTime.HasValue)
			{
				return Array.Empty<Measurement<double>>();
			}

			// record the measurement and reset invokeVoidAsyncTime
			var measurement = new Measurement<double>(invokeVoidAsyncTime.Value);
			invokeVoidAsyncTime = null;
			return new[] { measurement };
		});
	}

	public void RecordPing(double? value)
	{
		invokeVoidAsyncTime = value;
	}

	public void Dispose()
	{
		meter.Dispose();
		ActivitySource.Dispose();
	}
}
