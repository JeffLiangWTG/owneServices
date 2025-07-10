using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace CargoWise.Winzor.Telemetry.Test;

public class TelemetryConfigTest
{
	[Test]
	public void TestLoadConfig()
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(appsettings));
		var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
		var options = config.GetSection(nameof(TelemetryOptions)).Get<TelemetryOptions>();
		Assert.That(options, Is.Not.Null);
		Assert.That(options!.TraceEnableProbability, Is.EqualTo(1f));
		Assert.That(options!.MetricEnableProbability, Is.EqualTo(1f));
		Assert.That(options.ServiceName, Is.EqualTo("CargoWise.Winzor.AppServer"));
		Assert.That(options.OtlpEndpoint, Is.EqualTo("http://localhost:8200"));
		Assert.That(options.Sources, Has.Count.EqualTo(1));
		Assert.That(options.Sources[0].Name, Is.EqualTo("CargoWise.Winzor.*"));
		Assert.That(options.Sources[0].Enabled);
		Assert.That(options.SqlClientInstrumentation);
		Assert.That(options.ResourceAttributes?.Count, Is.EqualTo(2));
		Assert.That(options.ResourceAttributes?["service.owner.customerId"], Is.EqualTo("WTG"));
		Assert.That(options.ResourceAttributes?["service.owner.team"], Is.EqualTo("WZR"));
	}

	[TestCase("Development", "Development")]
	[TestCase("Production", "Production")]
	[TestCase("IntegrationTest", "Test")]
	[TestCase("DAT", "Test")]
	[TestCase("SampleEnv", "SampleEnv")]
	public void TestConfigureOpenTelemetry(string testEnv, string deploymentEnvironment)
	{
		var services = new ServiceCollection();
		var options = new TelemetryOptions()
		{
			TraceEnableProbability = 1,
			MetricEnableProbability = 1,
			ServiceName = "ServiceName",
			OtlpEndpoint = "http://localhost",
			Sources = new[]
			{
				new TelemetryOptions.Source() { Name = "Source1", Enabled = true },
				new TelemetryOptions.Source() { Name = "Source2", Enabled = false }
			},
			ServiceVersion = "ServiceVersion",
			ServiceInstanceId = "ServiceInstanceId",
			CargoWiseDbName = "CargoWiseDbName",
			SqlClientInstrumentation = false,
		};

		services.AddTelemetry(options, testEnv);

		var provider = services.BuildServiceProvider();
		var tracer = provider.GetRequiredService<TracerProvider>();
		var metrics = provider.GetRequiredService<MeterProvider>();
		var resource = tracer.GetResource();
		var attributes = resource.Attributes.ToDictionary(p => p.Key, p => p.Value);

		var exporterOptions = provider.GetRequiredService<IOptionsMonitor<OtlpExporterOptions>>();
		var traceExporterOption = exporterOptions.Get("trace");
		var metricExporterOption = exporterOptions.Get("metric");

		Assert.Multiple(() =>
		{
			Assert.That(attributes, Does.ContainKey("service.name"));
			Assert.That(attributes["service.name"], Is.EqualTo("ServiceName"));
			Assert.That(attributes, Does.ContainKey("service.version"));
			Assert.That(attributes["service.version"], Is.EqualTo("ServiceVersion"));
			Assert.That(attributes, Does.ContainKey("service.instance.id"));
			Assert.That(attributes["service.instance.id"], Is.EqualTo("ServiceInstanceId"));
			Assert.That(attributes, Does.ContainKey("cargowise.dbname"));
			Assert.That(attributes["cargowise.dbname"], Is.EqualTo("CargoWiseDbName"));
			Assert.That(attributes, Does.ContainKey("host.name"));
			Assert.That(attributes["host.name"], Is.EqualTo(Environment.MachineName));
			Assert.That(attributes, Does.ContainKey("deployment.environment"));
			Assert.That(attributes["deployment.environment"], Is.EqualTo(deploymentEnvironment));
			Assert.That(attributes, Does.ContainKey("otel_auth_token"));
			Assert.That(traceExporterOption.Protocol, Is.EqualTo(OtlpExportProtocol.HttpProtobuf));
			Assert.That(traceExporterOption.Endpoint.ToString(), Is.EqualTo("http://localhost/v1/traces"));
			Assert.That(traceExporterOption.Headers, Contains.Substring("Authorization=Bearer"));
			Assert.That(metricExporterOption.Protocol, Is.EqualTo(OtlpExportProtocol.HttpProtobuf));
			Assert.That(metricExporterOption.Endpoint.ToString(), Is.EqualTo("http://localhost/v1/metrics"));
			Assert.That(metricExporterOption.Headers, Contains.Substring("Authorization=Bearer"));
		});
	}

	[TestCase(0f, false, 0f, false)]
	[TestCase(1f, true, 1f, true)]
	[TestCase(0.3f, true, 0.5f, true)]
	[TestCase(0.2f, false, 0.4f, false)]
	[TestCase(0.2f, false, 0.5f, true)]
	[TestCase(0.3f, true, 0.4f, false)]
	public void TestEnableRandomly(float traceProbability, bool enableTrace, float metricProbability, bool enableMetric)
	{
		var services = new ServiceCollection();
		var options = new TelemetryOptions()
		{
			TraceEnableProbability = traceProbability,
			MetricEnableProbability = metricProbability,
			//md5 a7232d8c81c66c1623f3c43e9104eae5
			//rand seed = 0xa7232d8c
			//r[0], r[1] = 0.22, 0.45 
			ServiceInstanceId = "ServiceInstanceId",
		};

		services.AddTelemetry(options, "Test");

		var provider = services.BuildServiceProvider();
		var tracer = provider.GetService<TracerProvider>();
		var metrics = provider.GetService<IMetricsListener>();
		Assert.That(tracer is not null, Is.EqualTo(enableTrace));
		Assert.That(metrics is not null, Is.EqualTo(enableMetric));
	}

	const string appsettings = @"
{
	""TelemetryOptions"": {
		""TraceEnableProbability"": 1,
		""MetricEnableProbability"": 1,
		""ServiceName"": ""CargoWise.Winzor.AppServer"",
		""OtlpEndpoint"": ""http://localhost:8200"",
		""Sources"": [
			{
				""Name"": ""CargoWise.Winzor.*"",
				""Enabled"": true
			}
		],
		""SqlClientInstrumentation"": true,
		""MetricExportInterval"": ""00:01:00"",
        ""ResourceAttributes"": {
			""service.owner.customerId"": ""WTG"",
			""service.owner.team"": ""WZR""
		}
	}
}
";
}
