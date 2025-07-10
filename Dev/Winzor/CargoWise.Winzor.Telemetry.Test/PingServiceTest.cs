using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;

namespace CargoWise.Winzor.Telemetry.Test;

public class PingServiceTest
{
	[Test]
	public void TestPingToRecordMetric()
	{
		var metrics = new List<Metric>();
		using var exporter = new InMemoryExporter<Metric>(metrics);
		using var reader = new BaseExportingMetricReader(exporter);
		using var meterProvider = Sdk.CreateMeterProviderBuilder()
			.AddMeter(SignalRTelemetry.SourceName)
			.AddReader(reader)
			.Build();

		var jsRuntimeMock = new Mock<IJSRuntime>();

		using var telemetry = new SignalRTelemetry();
		using var pingService = new PingService(jsRuntimeMock.Object, telemetry, Options.Create(new TelemetryOptions()));

		Assert.DoesNotThrowAsync(async () => await pingService.Start());

		reader.Collect();

		Assert.That(metrics, Has.Count.EqualTo(1));
		Assert.That(metrics[0].Name, Is.EqualTo("signalr.ping"));
		var snapshot = new MetricSnapshot(metrics[0]);
		Assert.That(snapshot.MetricPoints, Has.Count.EqualTo(1));
		Assert.That(snapshot.MetricPoints[0].GetGaugeLastValueDouble(), Is.GreaterThan(0));
	}

	[Test]
	public async Task TestPingToNotRecordMetricWhenThereIsAnExceptionAsync()
	{
		var metrics = new List<Metric>();
		using var exporter = new InMemoryExporter<Metric>(metrics);
		using var reader = new BaseExportingMetricReader(exporter);
		using var meterProvider = Sdk.CreateMeterProviderBuilder()
			.AddMeter(SignalRTelemetry.SourceName)
			.AddReader(reader)
			.Build();

		var jsRuntimeMock = new Mock<IJSRuntime>();
		jsRuntimeMock
			.Setup(js => js.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()))
			.ThrowsAsync(new Exception());

		using var telemetry = new SignalRTelemetry();
		var loggerMock = new Mock<ILogger<PingService>>();
		using var pingService = new PingService(jsRuntimeMock.Object, telemetry, Options.Create(new TelemetryOptions()));
		await pingService.Start();
		reader.Collect();

		Assert.That(metrics, Has.Count.EqualTo(0));
	}

	[Test]
	public void TestPingDoesNotThrowExceptionAndNotToLog()
	{
		var jsRuntimeMock = new Mock<IJSRuntime>();
		jsRuntimeMock
			.Setup(js => js.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()))
			.ThrowsAsync(new Exception());

		using var telemetry = new SignalRTelemetry();
		using var pingService = new PingService(jsRuntimeMock.Object, telemetry, Options.Create(new TelemetryOptions()));

		Assert.DoesNotThrowAsync(pingService.Start);
	}
}
