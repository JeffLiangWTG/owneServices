using System.Diagnostics;

namespace WinzorFramework.Telemetry;

public static class TelemetryService
{
	public static readonly ActivitySource ActivitySource = new ActivitySource("WinzorFramework");
}
