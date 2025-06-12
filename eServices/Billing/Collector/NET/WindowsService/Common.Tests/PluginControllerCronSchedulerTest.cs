using System.Collections;
using System.Reflection;
using CargoWise.Billing.API;
using CargoWise.Billing.Client;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.Billing.CollectorService.Plugin.Tests;
using CargoWise.Billing.Kafka.API;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common.Tests
{
	[TestFixture]
	public class PluginControllerCronSchedulerTest
	{
		[Test]
		public void TestCronSchedulerNextRunDueTimeDefault()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);

			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-10).AddTicks(4961);
			var lastSuccessfulRun = now.AddMinutes(-10).AddTicks(4961);

			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				new TimeStampedTransaction(now.AddMinutes(-6), new BillingTransaction()),
				new TimeStampedTransaction(now.AddMinutes(-5), new BillingTransaction()),
				new TimeStampedTransaction(now.AddMinutes(-7), new BillingTransaction()),
			};

			var clock = new TestClock() { UtcNow = now };
			using var controller = CreatePluginController(pluginType.Assembly, settings, state, clock);
			controller.Start();
			var plugin = DummyBillingTransactionsPlugin.LastInstance;
			Assert.AreEqual(0, plugin.FoundTransactions.Count());

			var infoLogMessagesList = InMemorySink.Instance.LogEvents.Where(e => e.Level == LogEventLevel.Information).ToList();
			Assert.That(infoLogMessagesList[0].RenderMessage(), Is.EqualTo("Next run scheduled in 00:00:00:01"));

			clock.UtcNow = now.AddSeconds(1);
			Assert.AreEqual(3, plugin.FoundTransactions.Count());

			var end = lastSuccessfulRun + settings.Interval;
			var infoLogMessagesList2 = InMemorySink.Instance.LogEvents.Where(e => e.Level == LogEventLevel.Information).ToList();
			Assert.That(infoLogMessagesList2.Count, Is.EqualTo(4));
			Assert.That(infoLogMessagesList2[0].RenderMessage(), Is.EqualTo("Next run scheduled in 00:00:00:01"));
			Assert.That(infoLogMessagesList2[1].RenderMessage(), Is.EqualTo("Transactions being retrieved for period '" + lastSuccessfulRun + "' (exclusive) to '" + end + "'(inclusive)"));
			Assert.That(infoLogMessagesList2[2].RenderMessage(), Is.EqualTo("3 billing transactions found. 3 submitted successfully. 0 in error. "));
			Assert.That(infoLogMessagesList2[3].RenderMessage(), Is.EqualTo("Next run scheduled in 00:00:09:59"));
		}

		[Test]
		public void TestCronSchedulerNextRunDueTimeCron()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);

			var now = new DateTime(2024, 9, 1, 0, 3, 0, DateTimeKind.Utc);
			var lastTransactionTimestamp = now.AddMinutes(-10).AddTicks(4961);
			var lastSuccessfulRun = now.AddMinutes(-3).AddTicks(4961);

			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true, typeof(PluginCronScheduler).FullName);
			settings.CronSettings = new CronSettings()
			{
				Expression = "*/5 * * * *",
				TimezoneId = "AUS Eastern Standard Time"
			};
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			var clock = new TestClock() { UtcNow = now };
			using var controller = CreatePluginController(pluginType.Assembly, settings, state, clock);
			controller.Start();
			var plugin = DummyBillingTransactionsPlugin.LastInstance;
			Assert.AreEqual(0, plugin.FoundTransactions.Count());

			var infoLogMessagesList = InMemorySink.Instance.LogEvents.Where(e => e.Level == LogEventLevel.Information).ToList();
			Assert.That(infoLogMessagesList[0].RenderMessage(), Is.EqualTo("Next run scheduled in 00:00:02:00"));
		}

		[Test]
		public void TestCronSchedulerNextRunDueTimeCron_ResetLastRun()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);

			var now = new DateTime(2024, 9, 3, 0, 3, 0, DateTimeKind.Utc);
			var lastTransactionTimestamp = now.AddMinutes(-10).AddTicks(4961);
			var lastSuccessfulRun = now.AddMinutes(-19).AddTicks(4961);

			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true, typeof(PluginCronScheduler).FullName);
			settings.CronSettings = new CronSettings()
			{
				Expression = "*/5 * * * *",
				TimezoneId = "AUS Eastern Standard Time"
			};
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			var clock = new TestClock() { UtcNow = now };
			using var controller = CreatePluginController(pluginType.Assembly, settings, state, clock);
			controller.Start();
			var plugin = DummyBillingTransactionsPlugin.LastInstance;
			Assert.AreEqual(0, plugin.FoundTransactions.Count());

			var expectedRanges = new []
			{
				new []{ new DateTime(2024, 9, 2, 23, 44, 0), new DateTime(2024, 9, 2, 23, 45, 0) },
				new []{ new DateTime(2024, 9, 2, 23, 45, 0), new DateTime(2024, 9, 2, 23, 50, 0) },
				new []{ new DateTime(2024, 9, 2, 23, 50, 0), new DateTime(2024, 9, 2, 23, 55, 0) },
				new []{ new DateTime(2024, 9, 2, 23, 55, 0), new DateTime(2024, 9, 3, 0, 0, 0) },
			};

			var infoLogMessagesList = InMemorySink.Instance.LogEvents.Where(e => e.Level == LogEventLevel.Information).ToList();
			Assert.That(infoLogMessagesList.Count, Is.EqualTo(10));
			Assert.That(infoLogMessagesList[0].RenderMessage(), Is.EqualTo("Next run scheduled in 00:00:00:00"));
			Assert.That(infoLogMessagesList[1].RenderMessage(), Is.EqualTo($"Transactions being retrieved for period '{expectedRanges[0][0]}' (exclusive) to '{expectedRanges[0][1]}'(inclusive)"));
			Assert.That(infoLogMessagesList[2].RenderMessage(), Is.EqualTo("0 billing transactions found. 0 submitted successfully. 0 in error. "));
			Assert.That(infoLogMessagesList[3].RenderMessage(), Is.EqualTo($"Transactions being retrieved for period '{expectedRanges[1][0]}' (exclusive) to '{expectedRanges[1][1]}'(inclusive)"));
			Assert.That(infoLogMessagesList[4].RenderMessage(), Is.EqualTo("0 billing transactions found. 0 submitted successfully. 0 in error. "));
			Assert.That(infoLogMessagesList[5].RenderMessage(), Is.EqualTo($"Transactions being retrieved for period '{expectedRanges[2][0]}' (exclusive) to '{expectedRanges[2][1]}'(inclusive)"));
			Assert.That(infoLogMessagesList[6].RenderMessage(), Is.EqualTo("0 billing transactions found. 0 submitted successfully. 0 in error. "));
			Assert.That(infoLogMessagesList[7].RenderMessage(), Is.EqualTo($"Transactions being retrieved for period '{expectedRanges[3][0]}' (exclusive) to '{expectedRanges[3][1]}'(inclusive)"));
			Assert.That(infoLogMessagesList[8].RenderMessage(), Is.EqualTo("0 billing transactions found. 0 submitted successfully. 0 in error. "));
			Assert.That(infoLogMessagesList[9].RenderMessage(), Is.EqualTo("Next run scheduled in 00:00:02:00"));
		}

		[Test]
		public void TestCronSchedulerNextRunDueTimeCron_CollectFromPreviousOccurrence()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);

			var now = new DateTime(2024, 9, 3, 0, 0, 0, DateTimeKind.Utc);
			var lastTransactionTimestamp = new DateTime(2024, 7, 15, 1, 2, 3, DateTimeKind.Utc);
			var lastSuccessfulRun = new DateTime(2024, 7, 15, 1, 2, 3, DateTimeKind.Utc);

			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true, typeof(PluginCronScheduler).FullName);
			settings.CronSettings = new CronSettings()
			{
				Expression = "0 0 1 * *",
				TimezoneId = "AUS Eastern Standard Time",
				CollectFromPreviousOccurrence = true
			};
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			var clock = new TestClock() { UtcNow = now };
			using var controller = CreatePluginController(pluginType.Assembly, settings, state, clock);
			controller.Start();
			var plugin = DummyBillingTransactionsPlugin.LastInstance;
			Assert.AreEqual(0, plugin.FoundTransactions.Count());

			var expectedRanges = new[]
			{
				new []{ new DateTime(2024, 6, 30, 14, 0, 0), new DateTime(2024, 7, 31, 14, 0, 0) },
				new []{ new DateTime(2024, 7, 31, 14, 0, 0), new DateTime(2024, 8, 31, 14, 0, 0) },
			};

			var infoLogMessagesList = InMemorySink.Instance.LogEvents.Where(e => e.Level == LogEventLevel.Information).ToList();
			Assert.That(infoLogMessagesList.Count, Is.EqualTo(6));
			Assert.That(infoLogMessagesList[0].RenderMessage(), Is.EqualTo("Next run scheduled in 00:00:00:00"));
			Assert.That(infoLogMessagesList[1].RenderMessage(), Is.EqualTo($"Transactions being retrieved for period '{expectedRanges[0][0]}' (exclusive) to '{expectedRanges[0][1]}'(inclusive)"));
			Assert.That(infoLogMessagesList[2].RenderMessage(), Is.EqualTo("0 billing transactions found. 0 submitted successfully. 0 in error. "));
			Assert.That(infoLogMessagesList[3].RenderMessage(), Is.EqualTo($"Transactions being retrieved for period '{expectedRanges[1][0]}' (exclusive) to '{expectedRanges[1][1]}'(inclusive)"));
			Assert.That(infoLogMessagesList[4].RenderMessage(), Is.EqualTo("0 billing transactions found. 0 submitted successfully. 0 in error. "));
			Assert.That(infoLogMessagesList[5].RenderMessage(), Is.EqualTo("Next run scheduled in 27:14:00:00"));
		}

		[Test]
		[TestCaseSource(nameof(KnownExceptionRetryTestCases))]
		public string TestCronSchedulerNextRunDueTimeCron_KnownException_Retry(DateTime now, DateTime lastSuccessfulRun)
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var lastTransactionTimestamp = lastSuccessfulRun.AddMonths(-1);

			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true, typeof(PluginCronScheduler).FullName);
			settings.CronSettings = new CronSettings()
			{
				Expression = "0 0 1 * *",
				TimezoneId = "AUS Eastern Standard Time",
				CollectFromPreviousOccurrence = true
			};
			settings.MaxRetryAttempts = 0;
			settings.IntervalMinutes = 1;
			settings.RetryIntervalSeconds = 5;
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			var billingTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = lastSuccessfulRun.AddDays(1),
				ReportingSource = "TST"
			};
			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				new TimeStampedTransaction(lastSuccessfulRun.AddDays(1), billingTransaction)
			};

			clientMock.Setup(_ => _.AddTransaction(billingTransaction)).Throws(new TimeoutException());

			var knownExceptions = new Dictionary<string, string[]>()
			{
				{typeof(TimeoutException).FullName, null}
			};

			var clock = new TestClock() { UtcNow = now };
			using var controller = CreatePluginController(pluginType.Assembly, settings, state, clock, knownExceptions);
			controller.Start();

			var expectedRanges = new[]
			{
				new []{ new DateTime(2024, 6, 30, 14, 0, 0), new DateTime(2024, 7, 31, 14, 0, 0) },
			};

			var messages = InMemorySink.Instance.LogEvents.Select(x => x.RenderMessage()).ToList();
			Assert.That(messages.Count, Is.EqualTo(4));
			Assert.That(messages[0], Is.EqualTo("Next run scheduled in 00:00:00:00"));
			Assert.That(messages[1], Is.EqualTo($"Transactions being retrieved for period '{expectedRanges[0][0]}' (exclusive) to '{expectedRanges[0][1]}'(inclusive)"));
			Assert.That(messages[2], Does.StartWith("Error happened while collecting and sending transactions."));
			return messages[3];
		}

		[Test]
		public void TestCronSchedulerNextRunDueTimeCron_CollectFromPreviousOccurrence_WithNextOccurrenceDelayInMinutes()
		{
			 var pluginType = typeof(DummyBillingTransactionsPlugin);

			var now = new DateTime(2025, 3, 1, 13, 0, 0, DateTimeKind.Utc);  // This is 1st Mar 00:00 AEST
			var lastSuccessfulRun = new DateTime(2024, 7, 31, 14, 0, 0, DateTimeKind.Utc); // This is 1st Aug 00:00 AEST

			var settings =
				PluginControllerTestHelper.PluginControllerSettings(pluginType, true,
					typeof(PluginCronScheduler).FullName);
			settings.CronSettings = new CronSettings()
			{
				Expression = "0 0 1 * *",
				TimezoneId = "AUS Eastern Standard Time",
				CollectFromPreviousOccurrence = true,
				NextOccurrenceDelayInMinutes = "360"
			};

			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun,
				lastSuccessfulRun);

			var clock = new TestClock() { UtcNow = now };
			using var controller = CreatePluginController(pluginType.Assembly, settings, state, clock);
			controller.Start();
			var plugin = DummyBillingTransactionsPlugin.LastInstance;
			var expectedRanges = new[]
			{
				new[] { new DateTime(2024, 7, 31, 14, 0, 0), new DateTime(2024, 8, 31, 14, 0, 0) },
				new[] { new DateTime(2024, 8, 31, 14, 0, 0), new DateTime(2024, 9, 30, 14, 0, 0) },
				new[] { new DateTime(2024, 9, 30, 14, 0, 0), new DateTime(2024, 10, 31, 13, 0, 0) },
				new[] { new DateTime(2024, 10, 31, 13, 0, 0), new DateTime(2024, 11, 30, 13, 0, 0) },
				new[] { new DateTime(2024, 11, 30, 13, 0, 0), new DateTime(2024, 12, 31, 13, 0, 0) },
				new[] { new DateTime(2024, 12, 31, 13, 0, 0), new DateTime(2025, 1, 31, 13, 0, 0) },
				new[] { new DateTime(2025, 1, 31, 13, 0, 0), new DateTime(2025, 2, 28, 13, 0, 0) },
			};
			Assert.AreEqual(0, plugin.FoundTransactions.Count());

			var infoLogMessagesList = InMemorySink.Instance.LogEvents
				.Where(e => e.Level == LogEventLevel.Information)
				.Select(x => x.RenderMessage())
				.ToList();

			Assert.That(infoLogMessagesList.Count, Is.EqualTo(16));

			Assert.That(infoLogMessagesList[0], Is.EqualTo("Next run scheduled in 00:00:00:00"));

			for (int i = 0; i < expectedRanges.Length; i++)
			{
				Assert.That(infoLogMessagesList[1 + i * 2],
					Is.EqualTo($"Transactions being retrieved for period '{expectedRanges[i][0]}' (exclusive) to '{expectedRanges[i][1]}'(inclusive)"));
				Assert.That(infoLogMessagesList[2 + i * 2],
					Is.EqualTo("0 billing transactions found. 0 submitted successfully. 0 in error. "));
			}
			Assert.That(infoLogMessagesList[15], Is.EqualTo("Next run scheduled in 30:06:00:00"));  //30d 6h delay

		}

		PluginController CreatePluginController(Assembly assembly, PluginControllerSettings settings, PluginControllerState state, TestClock clock, Dictionary<string, string[]> knownExceptions = null) => new (assembly, settings, state,
			globalSettings, clientMock.Object, kafkaClientMock.Object, clock, new object(), loggerFactory, errorReportingClientMock.Object, knownExceptions, new []{ typeof(PluginCronScheduler) });

		[SetUp]
		public void SetUp()
		{
			clientMock = new Mock<IBillingServiceClient>();
			kafkaClientMock = new Mock<IBillingKafkaClient>();
			errorReportingClientMock = new Mock<IErrorReportingClient>();
			errorReportingClientMock.Setup(x => x.ServiceUri).Returns(new Uri("", UriKind.Relative));
			var logger = new LoggerConfiguration()
				.WriteTo.InMemory()
				.CreateLogger();
			loggerFactory = new LoggerFactory().AddSerilog(logger);
		}

		Mock<IBillingServiceClient> clientMock;
		Mock<IBillingKafkaClient> kafkaClientMock;
		Dictionary<string, string> globalSettings = new()
		{
			{ "UsageELKKafkaTopic", "TestTopic" }
		};
		ILoggerFactory loggerFactory;
		Mock<IErrorReportingClient> errorReportingClientMock;

		static IEnumerable KnownExceptionRetryTestCases
		{
			get
			{
				yield return new TestCaseData(new DateTime(2024, 9, 3, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 7, 15, 1, 2, 3, DateTimeKind.Utc))
					.SetName("TestCronSchedulerNextRunDueTimeCron_KnownException_RetryInterval")
					.Returns("Next run scheduled in 00:00:00:05");
				yield return new TestCaseData(new DateTime(2024, 8, 31, 13, 59, 58, DateTimeKind.Utc), new DateTime(2024, 7, 15, 1, 2, 3, DateTimeKind.Utc))
					.SetName("TestCronSchedulerNextRunDueTimeCron_KnownException_CronInterval")
					.Returns("Next run scheduled in 00:00:00:02");
			}
		}
	}
}
