using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Winzor.Telemetry;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;

namespace CargoWise.Winzor.AppServer.Test
{
	public class ShutDownCircuitHandlerTests
	{
		[Test]
		public void ShutdownCalledWhenNoConnectionEverConnected()
		{
			Assert.That(VerifyHostStopAsyncCalled, Throws.Nothing.After(2).Seconds.PollEvery(100).MilliSeconds);
		}

		[Test]
		public async Task ShutdownCalledAfterSingleConnectionClosed()
		{
			await tracker.OnCircuitOpenedAsync("One");
			await tracker.OnCircuitClosedAsync("One");
			VerifyHostStopAsyncCalled();
		}

		void VerifyHostStopAsyncCalled()
		{
			hostMock.Verify(h => h.StopAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ShutdownCalledAfterMultipleConnectionsClosed()
		{
			await tracker.OnCircuitOpenedAsync("One");
			await tracker.OnCircuitOpenedAsync("Two");
			await tracker.OnCircuitClosedAsync("One");
			hostMock.Verify(h => h.StopAsync(CancellationToken.None), Times.Never);
			await tracker.OnCircuitClosedAsync("Two");
			hostMock.Verify(h => h.StopAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ShutdownCanceledAfterConnectOnce()
		{
			await tracker.OnCircuitOpenedAsync("One");
			await Task.Delay(TimeSpan.FromSeconds(2));
			hostMock.Verify(h => h.StopAsync(It.IsAny<CancellationToken>()), Times.Never);
		}

		[Test]
		public async Task ShutdownSingleCircuitByMultipleThread()
		{
			await tracker.OnCircuitOpenedAsync("One");
			await tracker.OnCircuitOpenedAsync("Two");
			await tracker.OnCircuitOpenedAsync("Three");

			Task closeCircuitForOne = Task.Run(() => tracker.OnCircuitClosedAsync("One"));
			Task closeCircuitForOneWithThread2 = Task.Run(() => tracker.OnCircuitClosedAsync("One"));
			Task closeCircuitForOneWithThread3 = Task.Run(() => tracker.OnCircuitClosedAsync("One"));
			await Task.WhenAll(closeCircuitForOne, closeCircuitForOneWithThread2, closeCircuitForOneWithThread3);

			Task closeCircuitForTwo = Task.Run(() => tracker.OnCircuitClosedAsync("Two"));
			Task closeCircuitForTwoWithThread2 = Task.Run(() => tracker.OnCircuitClosedAsync("Two"));
			await Task.WhenAll(closeCircuitForTwo, closeCircuitForTwoWithThread2);
		}

		[Test]
		public async Task VerifyLogTrace()
		{
			await tracker.OnCircuitOpenedAsync("testCircuitId");

			loggerMock.Verify(
				x => x.Log(
					It.Is<LogLevel>(l => l == LogLevel.Trace),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "New circuit, abandoning shutdown timer (if running)"),
					It.IsAny<Exception>(),
					It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
				Times.AtLeastOnce);
		}

		[Test]
		public async Task DebugMessageLoggedWhenNewCircuitOpened()
		{
			await tracker.OnCircuitOpenedAsync("testCircuitId");

			loggerMock.Verify(
				x => x.Log(
					It.Is<LogLevel>(l => l == LogLevel.Debug),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Circuit opened (testCircuitId)"),
					It.IsAny<Exception>(),
					It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
				Times.Once);
		}

		[Test]
		public async Task DebugMessageLoggedWhenSameCircuitOpened()
		{
			await tracker.OnCircuitOpenedAsync("testCircuitId");
			loggerMock.Invocations.Clear();
			await tracker.OnCircuitOpenedAsync("testCircuitId");

			loggerMock.Verify(
				x => x.Log(
					It.Is<LogLevel>(l => l == LogLevel.Debug),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Circuit re-opened (testCircuitId)"),
					It.IsAny<Exception>(),
					It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
				Times.Once);
		}

		[Test]
		public async Task DebugMessageLoggedWhenKnownCircuitClosed()
		{
			await tracker.OnCircuitOpenedAsync("testCircuitId");
			loggerMock.Invocations.Clear();
			await tracker.OnCircuitClosedAsync("testCircuitId");

			loggerMock.Verify(
				x => x.Log(
					It.Is<LogLevel>(l => l == LogLevel.Debug),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Circuit closed (testCircuitId)"),
					It.IsAny<Exception>(),
					It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
				Times.Once);
		}

		[Test]
		public async Task DebugMessageLoggedWhenUnknownCircuitClosed()
		{
			await tracker.OnCircuitClosedAsync("testCircuitId");

			loggerMock.Verify(
				x => x.Log(
					It.Is<LogLevel>(l => l == LogLevel.Debug),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Circuit closed but was never opened (testCircuitId)"),
					It.IsAny<Exception>(),
					It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
				Times.Once);
		}

		[Test]
		public async Task InformationMessageLoggedWhenAllCircuitsClosed()
		{
			var verifyLoggedMessage = (Func<Times> times) => loggerMock.Verify(
				x => x.Log(
					It.Is<LogLevel>(l => l == LogLevel.Information),
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "All circuits closed"),
					It.IsAny<Exception>(),
					It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
				times);

			await tracker.OnCircuitOpenedAsync("testCircuitId1");
			await tracker.OnCircuitOpenedAsync("testCircuitId2");
			await tracker.OnCircuitClosedAsync("testCircuitId2");
			await tracker.OnCircuitOpenedAsync("testCircuitId3");
			await tracker.OnCircuitClosedAsync("testCircuitId1");
			verifyLoggedMessage(Times.Never);
			await tracker.OnCircuitClosedAsync("testCircuitId3");
			verifyLoggedMessage(Times.Once);
		}

		[Test]
		public async Task IncrementDisconnectCounterOnConnectionDownAsync()
		{
			var metrics = new List<Metric>();
			using var inMemoryExporter = new InMemoryExporter<Metric>(metrics);
			using var reader = new BaseExportingMetricReader(inMemoryExporter);
			using var meterProvider = Sdk.CreateMeterProviderBuilder()
				.AddMeter("*")
				.AddReader(reader)
				.Build();

			using var telemetry = new CircuitTelemetry();
			var cancellationToken = CancellationToken.None;
			var shutDownCircuitHandler = new ShutDownCircuitHandler(loggerMock.Object, application, cwOptions, telemetry);

			await shutDownCircuitHandler.OnConnectionDownAsync(null, cancellationToken);
			await shutDownCircuitHandler.OnConnectionDownAsync(null, cancellationToken);
			await shutDownCircuitHandler.OnConnectionDownAsync(null, cancellationToken);

			reader.Collect();

			Assert.That(metrics, Has.Count.EqualTo(1));
			Assert.That(metrics[0].Name, Is.EqualTo("circuit.disconnects"));
			var snapshot = new MetricSnapshot(metrics[0]);
			Assert.That(snapshot.MetricPoints, Has.Count.EqualTo(1));
			Assert.That(snapshot.MetricPoints[0].GetSumLong(), Is.EqualTo(3));
		}

		[Test]
		public async Task IncrementCloseCounterOnOnCircuitClosedAsync()
		{
			var metrics = new List<Metric>();
			using var inMemoryExporter = new InMemoryExporter<Metric>(metrics);
			using var reader = new BaseExportingMetricReader(inMemoryExporter);
			using var meterProvider = Sdk.CreateMeterProviderBuilder()
				.AddMeter("*")
				.AddReader(reader)
				.Build();

			using var telemetry = new CircuitTelemetry();
			var cancellationToken = CancellationToken.None;
			var shutDownCircuitHandler = new ShutDownCircuitHandler(loggerMock.Object, application, cwOptions, telemetry);

			await shutDownCircuitHandler.OnCircuitClosedAsync(null, cancellationToken);
			await shutDownCircuitHandler.OnCircuitClosedAsync(null, cancellationToken);
			await shutDownCircuitHandler.OnCircuitClosedAsync(null, cancellationToken);

			reader.Collect();

			Assert.That(metrics, Has.Count.EqualTo(1));
			Assert.That(metrics[0].Name, Is.EqualTo("circuit.close"));
			var snapshot = new MetricSnapshot(metrics[0]);
			Assert.That(snapshot.MetricPoints, Has.Count.EqualTo(1));
			Assert.That(snapshot.MetricPoints[0].GetSumLong(), Is.EqualTo(3));
		}

		[Test]
		public async Task OnCircuitClosedAsync_ShouldCallShutdownAsync_OnlyOnce()
		{
			var tasks = new List<Task>();
			var circuitIdList = new List<string>();
			for (var i = 1; i <= 10; i++)
			{
				circuitIdList.Add($"testCircuitId{i}");
			}
			foreach (var circuitId in circuitIdList)
			{
				await tracker.OnCircuitOpenedAsync(circuitId);
			}
			using var barrier = new Barrier(circuitIdList.Count);
			foreach (var circuitId in circuitIdList)
			{
				tasks.Add(Task.Run(() =>
				{
					barrier.SignalAndWait(); // Ensure all threads start at the same time
					return tracker.OnCircuitClosedAsync(circuitId);
				}));
			}
			await Task.WhenAll(tasks);
			hostMock.Verify(h => h.StopAsync(CancellationToken.None), Times.Once);
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var options = new CargoWiseOptions()
			{
				CircuitNeverOpenedShutdownTimeLimit = TimeSpan.FromSeconds(1),
				DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(1),
			};
			var cwOptionsMock = new Mock<IOptions<CargoWiseOptions>>();
			cwOptionsMock.SetupGet(x => x.Value).Returns(options);
			cwOptions = cwOptionsMock.Object;
		}

		[SetUp]
		public void Setup()
		{
			loggerMock = new Mock<ILogger<ShutDownCircuitHandler>>();
			applicationLoggerMock = new Mock<ILogger<Application>>();
			hostMock = new Mock<IHost>();
			application = new Application(applicationLoggerMock.Object, hostMock.Object, Mock.Of<IHostApplicationLifetime>(), cwOptions);
			tracker = new CircuitTracker(loggerMock.Object, application, cwOptions);
		}

		Mock<IHost> hostMock;
		Application application;
		Mock<ILogger<ShutDownCircuitHandler>> loggerMock;
		Mock<ILogger<Application>> applicationLoggerMock;
		IOptions<CargoWiseOptions> cwOptions;
		CircuitTracker tracker;
	}
}
