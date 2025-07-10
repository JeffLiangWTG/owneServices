using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Text;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Common.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace CargoWise.Winzor.Telemetry.Test;

class TelemetryTest
{
	[Test]
	public async Task TestCreateTracesAsync()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		var sw = new Stopwatch();
		using var activitySource = new ActivitySource("test");
		using (activitySource.StartActivity("TestCreateTraces"))
		{
			sw.Start();
			await Task.Delay(100);
			sw.Stop();
		}

		Assert.That(traces, Has.Count.EqualTo(1));
		Assert.That(traces[0].OperationName, Is.EqualTo("TestCreateTraces"));
		Assert.That(traces[0].Duration.Milliseconds, Is.GreaterThanOrEqualTo(sw.ElapsedMilliseconds));
	}

	[Test]
	public void TestCreateMetrics()
	{
		var metrics = new List<Metric>();
		using var exporter = new InMemoryExporter<Metric>(metrics);
		using var reader = new BaseExportingMetricReader(exporter);
		using var meterProvider = Sdk.CreateMeterProviderBuilder()
			.AddMeter("TestCreateMetrics")
			.AddReader(reader)
			.Build();

		using var meter = new Meter("TestCreateMetrics");
		var counter = meter.CreateCounter<int>("counter");
		counter.Add(1);
		counter.Add(2);
		counter.Add(3);

		reader.Collect();

		Assert.That(metrics, Has.Count.EqualTo(1));
		Assert.That(metrics[0].Name, Is.EqualTo("counter"));
		var snapshot = new MetricSnapshot(metrics[0]);
		Assert.That(snapshot.MetricPoints, Has.Count.EqualTo(1));
		Assert.That(snapshot.MetricPoints[0].GetSumLong(), Is.EqualTo(6));
	}

	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
	async Task CreateSqlClientInstrumentationAsync(bool enableTextStatement, bool recordException, Func<List<Activity>, SqlConnection, Task> func)
	{
		var traces = new List<Activity>();

		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddSqlClientInstrumentation(options =>
			{
				options.SetDbStatementForText = enableTextStatement;
				options.RecordException = recordException;
			})
			.AddInMemoryExporter(traces)
			.Build();

		var config = new TestConfiguration();
		var cwOptions = ConfigurationBinder.Get<CargoWiseOptions>(config.GetSection("CargoWiseOptions")) ?? new CargoWiseOptions();
		Assert.That(string.IsNullOrWhiteSpace(cwOptions.DbServerName), Is.False);
		Assert.That(string.IsNullOrWhiteSpace(cwOptions.DatabaseName), Is.False);
		var connectionString = ConnectionStringBuilder.GetConnectionString(cwOptions.DbServerName, cwOptions.DatabaseName, "Telemetry.Test");
		Assert.That(string.IsNullOrWhiteSpace(connectionString), Is.False);
		using (var connection = new SqlConnection(connectionString))
		{
			await connection.OpenAsync();
			Assert.That(connection.State, Is.EqualTo(System.Data.ConnectionState.Open));
			await func(traces, connection);
		}
	}

	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
	[Test]
	public async Task TestSqlClientInstrumentation_DefaultOptionAsync()
	{
		await CreateSqlClientInstrumentationAsync(false, false,
			async (traces, connection) =>
			{
				await using (var command = new SqlCommand("SELECT 1", connection))
				{
					await command.ExecuteScalarAsync();
				}

				Assert.That(traces.Select(t => t.OperationName), Does.Contain("OpenTelemetry.Instrumentation.SqlClient.Execute"), "No SqlClient telemetry data collected.");
			});
	}

	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
	[Test]
	public async Task TestSqlClientInstrumentation_EnableTextStatementAsync()
	{
		await CreateSqlClientInstrumentationAsync(true, false,
			async (List<Activity> traces, SqlConnection connection) =>
			{
				using (var command = new SqlCommand("SELECT 1", connection))
				{
					await command.ExecuteScalarAsync();
				}

				var trace = traces.First(t => t.OperationName == "OpenTelemetry.Instrumentation.SqlClient.Execute");
				var dbStatement = trace.TagObjects.FirstOrDefault(tag => tag.Key.Equals("db.statement")).Value;
				Assert.That(dbStatement, Is.EqualTo("SELECT 1"));
			});
	}

	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
	[Test]
	public async Task TestSqlClientInstrumentation_EnableRecordExceptionAsync()
	{
		await CreateSqlClientInstrumentationAsync(false, true,
			async (List<Activity> traces, SqlConnection connection) =>
			{
				try
				{
					using (var command = new SqlCommand("INVALID COMMAND", connection))
					{
						await command.ExecuteScalarAsync();
					}
				}
				catch (Exception)
				{
					// do nothing
				}

				var trace = traces.First(t => t.OperationName == "OpenTelemetry.Instrumentation.SqlClient.Execute");
				Assert.That(trace.Events.First().Name, Is.EqualTo("exception"));
			});
	}

	[Test]
	public void TestTraceAction()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		using var activitySource = new ActivitySource("test");

		var action = () => Thread.Sleep(10);
		var tracedAction = activitySource.WithTracing(action, "name");

		Thread.Sleep(10);
		var thread = new Thread(() => tracedAction())
		{
			Name = "thread"
		};
		thread.Start();
		thread.Join();

		Assert.That(traces, Has.Count.EqualTo(1));
		Assert.That(traces[0].OperationName, Is.EqualTo("name"));
		Assert.That(traces[0].Duration.TotalMilliseconds, Is.GreaterThanOrEqualTo(10));
		Assert.That(traces[0].GetTagItem("thread"), Is.EqualTo("thread"));
		Assert.That(traces[0].GetTagItem("delay"), Is.GreaterThanOrEqualTo(10));
	}

	[TestCase("process.memory.usage")]
	[TestCase("process.cpu.time")]
	public void TestProcessMetricsExist(string metricName)
	{
		var metrics = new List<Metric>();
		using var exporter = new InMemoryExporter<Metric>(metrics);
		using var reader = new BaseExportingMetricReader(exporter);
		using var meterProvider = Sdk.CreateMeterProviderBuilder()
			.AddProcessInstrumentation()
			.AddReader(reader)
			.Build();

		reader.Collect();
		Assert.That(metrics.Any(m => m.Name == metricName), Is.True);
	}

	[TestCase("JS.BeginInvokeJS")]
	[TestCase("JS.RenderBatch")]
	public async Task TestTelemetryHubLifetimeManagerTraceAsync(string hubMethodName)
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		using var winzorTelemetry = new WinzorTelemetry();
		var manager = new TelemetryHubLifetimeManager<Hub>(winzorTelemetry, Mock.Of<ILogger<TelemetryHubLifetimeManager<Hub>>>());
		await manager.SendConnectionAsync("connectionId", hubMethodName, Array.Empty<object>());

		Assert.That(traces.Any(t => t.OperationName == $"{hubMethodName}"));
		Assert.That(traces.Any(t => t.TagObjects.Any(tag => tag.Key == "MessageSize" && Convert.ToInt32(tag.Value) > 0)));
	}

	[TestCase(10)]
	[TestCase(100)]
	[TestCase(1000)]
	[TestCase(10000)]
	[TestCase(100000)]
	public async Task TestEstimatedMessageSizeIsTolerableAsync(int stringArgLength)
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource("*")
			.AddInMemoryExporter(traces)
			.Build();

		var largeStringContent = new string('a', stringArgLength);
		var hubMethodArguments = new object[] {
			1001,
			true,
			9999.6666,
			largeStringContent,
			false,
			2001,
			new ArraySegment<byte>(Encoding.UTF8.GetBytes(largeStringContent))
		};
		var hubMethodName = "JS.BeginInvokeJS";

		var message = new InvocationMessage(hubMethodName, hubMethodArguments);
		var messagePackHubProtocol = new MessagePackHubProtocol();
		var messageSerializedBytes = messagePackHubProtocol.GetMessageBytes(message);
		var messageSerializedSize = messageSerializedBytes.Length;

		using var winzorTelemetry = new WinzorTelemetry();
		var manager = new TelemetryHubLifetimeManager<Hub>(winzorTelemetry, Mock.Of<ILogger<TelemetryHubLifetimeManager<Hub>>>());
		await manager.SendConnectionAsync("connectionId", hubMethodName, hubMethodArguments);

		Assert.That(traces.Any(t => t.OperationName == hubMethodName));

		double diff = traces
			.Where(t => t.OperationName == hubMethodName)
			.Select(t => Convert.ToInt32(t.TagObjects.First(tag => tag.Key == "MessageSize").Value))
			.FirstOrDefault() - messageSerializedSize;
		var diffIsTolerable = Math.Abs(diff) / messageSerializedSize <= 0.05 || diff <= 20;
		Assert.That(diffIsTolerable);
	}
}
