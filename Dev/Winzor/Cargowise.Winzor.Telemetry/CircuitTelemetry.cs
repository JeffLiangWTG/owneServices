using System.Diagnostics.Metrics;

namespace CargoWise.Winzor.Telemetry;

public class CircuitTelemetry : IDisposable
{
	const string SourceName = "CargoWise.Winzor.Connection";
	readonly Meter meter;
	readonly Counter<int> disconnectCounter;
	readonly Counter<int> closeCounter;

	public CircuitTelemetry()
	{
		meter = new (SourceName);
		disconnectCounter = meter.CreateCounter<int>("circuit.disconnects");
		closeCounter = meter.CreateCounter<int>("circuit.close");
	}

	public void IncrementDisconnectCounter()
	{
		disconnectCounter.Add(1);
	}

	public void IncrementCloseCounter()
	{
		closeCounter.Add(1);
	}

	public void Dispose()
	{
		meter.Dispose();
	}
}
