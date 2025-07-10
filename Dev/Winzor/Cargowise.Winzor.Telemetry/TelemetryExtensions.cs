using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CargoWise.Winzor.Telemetry;

public static class TelemetryExtensions
{
	const string TracePath = "/v1/traces";
	const string MetricPath = "/v1/metrics";
	public static void RegisterTelemetryServices(this IServiceCollection services)
	{
		services.AddSingleton<IWinzorTelemetry, WinzorTelemetry>();
		services.AddSingleton<CircuitTelemetry>();
		services.AddSingleton<ISignalRTelemetry, SignalRTelemetry>();
		services.AddScoped<IPingService, PingService>();
		services.AddSingleton(typeof(HubLifetimeManager<>), typeof(TelemetryHubLifetimeManager<>));
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Not UI string")]
	public static void AddTelemetry(this IServiceCollection services, TelemetryOptions options, string environmentName)
	{
		var authToken = "8XvJkL2oW9pQmT7g3YzAkH5EJxLWoX2a";
		var builder = services.AddOpenTelemetry().ConfigureResource(resource =>
		{
			var attributes = new Dictionary<string, object>();
			attributes.Add("service.name", options.ServiceName);
			if (!string.IsNullOrEmpty(options.ServiceVersion))
			{
				attributes.Add("service.version", options.ServiceVersion);
			}
			if (!string.IsNullOrEmpty(options.ServiceInstanceId))
			{
				attributes.Add("service.instance.id", options.ServiceInstanceId);
			}
			if (!string.IsNullOrEmpty(options.CargoWiseDbName))
			{
				attributes.Add("cargowise.dbname", options.CargoWiseDbName);
			}
			if (options.ResourceAttributes != null)
			{
				foreach (var attribute in options.ResourceAttributes)
				{
					attributes.Add(attribute.Key, attribute.Value);
				}
			}
			attributes.Add("host.name", Environment.MachineName);
			attributes.Add("deployment.environment", environmentName.ToDeploymentEnvironment());
			attributes.Add("otel_auth_token", authToken);
			resource.AddAttributes(attributes);
		});

		var sources = options.Sources.Where(s => s.Enabled).Select(s => s.Name).ToArray();
		var rand = SeedRandWithString(options.ServiceInstanceId);

		if (rand.NextDouble() < options.TraceEnableProbability)
		{
			builder.WithTracing(tracing =>
			{
				if (sources.Any())
				{
					tracing.AddSource(sources);
				}

				if (options.SqlClientInstrumentation)
				{
					tracing.AddSqlClientInstrumentation(options =>
					{
						options.SetDbStatementForText = true;
						options.RecordException = true;
					});
				}

				if (!string.IsNullOrEmpty(options.OtlpEndpoint))
				{
					tracing.AddOtlpExporter("trace", otlp =>
					{
						otlp.Endpoint = new Uri(options.OtlpEndpoint.TrimEnd('/') + TracePath);
						//Our collector had load balancing issues when grpc was used. Check with SRE before changing this.
						otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
						//Batch export bundles multiple traces into a single request. Sending individual requests for each trace will severely lag the server.
						otlp.ExportProcessorType = OpenTelemetry.ExportProcessorType.Batch;
						otlp.BatchExportProcessorOptions.ScheduledDelayMilliseconds = (int)options.TraceExportInterval.TotalMilliseconds;
						otlp.Headers = $"Authorization=Bearer {authToken}";
					});
				}
			});
		}

		if (rand.NextDouble() < options.MetricEnableProbability)
		{
			builder.WithMetrics(metric =>
			{
				if (sources.Any())
				{
					metric.AddMeter(sources);
				}

				if (!string.IsNullOrEmpty(options.OtlpEndpoint))
				{
					metric.AddOtlpExporter("metric", (otlp, reader) =>
					{
						otlp.Endpoint = new Uri(options.OtlpEndpoint.TrimEnd('/') + MetricPath);
						//Our collector had load balancing issues when grpc was used. Check with SRE before changing this.
						otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
						otlp.Headers = $"Authorization=Bearer {authToken}";
						//Makes counters restart from 0 after every export.
						//We want this because it will give us the relative metrics in each time period.
						reader.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
						reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = (int)options.MetricExportInterval.TotalMilliseconds;
					});
				}
				metric.AddProcessInstrumentation();
			});
		}
	}

	static Random SeedRandWithString(string seed)
	{
		var bytes = Encoding.ASCII.GetBytes(seed);
		using var md5 = MD5.Create();
		var hash = md5.ComputeHash(bytes).Take(4);
		if (BitConverter.IsLittleEndian)
		{
			hash = hash.Reverse();
		}
		var i = BitConverter.ToInt32(hash.ToArray(), 0);
		return new Random(i);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	static string ToDeploymentEnvironment(this string env) => env.ToLower() switch
	{
		"integrationtest" or "dat" => "Test",
		_ => env
	};
}
