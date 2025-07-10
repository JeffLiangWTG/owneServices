namespace CargoWise.Winzor.Telemetry;

public record TelemetryOptions
{
	/// <summary>
	/// Probability to enable tracing at startup
	/// 0 = always disabled, 1 = always enabled.
	/// </summary>
	public float TraceEnableProbability { get; init; }

	/// <summary>
	/// Probability to enable metrics at startup
	/// 0 = always disabled, 1 = always enabled.
	/// </summary>
	public float MetricEnableProbability { get; init; }
	public string ServiceName { get; init; } = string.Empty;
	public string? OtlpEndpoint { get; init; }

	/// <summary>
	/// Otlp trace export interval
	/// </summary>
	public TimeSpan TraceExportInterval { get; init; } = TimeSpan.FromSeconds(5);

	/// <summary>
	/// Otlp metric export interval
	/// </summary>
	public TimeSpan MetricExportInterval { get; init; } = TimeSpan.FromMinutes(1);
	public IReadOnlyList<Source> Sources { get; init; } = [];
	public string? ServiceVersion { get; set; }
	public string ServiceInstanceId { get; set; } = string.Empty;
	public string? CargoWiseDbName { get; set; }
	public bool SqlClientInstrumentation { get; set; }
	public Dictionary<string, string>? ResourceAttributes { get; set; }

	public record Source
	{
		public string Name { get; init; } = string.Empty;
		public bool Enabled { get; init; }
	}
}
