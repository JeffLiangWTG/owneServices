using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.Billing.CollectorService.Plugin.Tests;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests
{
	[TestFixture]
	class BillingServiceTest
	{
		[Test]
		public void TestStartAndStop()
		{
			var settingsFileName = "plugins_settings.xml";
			var stateFileName = "plugins_state.xml";
			var settingsFilePath = Path.Combine(Path.GetDirectoryName(typeof(BillingService).Assembly.Location), settingsFileName);
			var stateFilePath = Path.Combine(Path.GetDirectoryName(typeof(BillingService).Assembly.Location), stateFileName);

			var mockPluginsManager = new Mock<IPluginsManager>();
			var mockBillingService = new Mock<BillingService>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), settingsFileName, stateFileName, loggerFactory) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(settingsFilePath, stateFilePath)).Returns(mockPluginsManager.Object);

			var billingService = mockBillingService.Object;
			Start(billingService);

			mockPluginsManager.Verify(_ => _.Start());
			mockPluginsManager.Verify(_ => _.Dispose(), Times.Never);

			billingService.Stop();

			mockPluginsManager.Verify(_ => _.Dispose());
		}

		[Test]
		public void TestPluginStartThrowsException_ShouldReportToIssueManager()
		{
			var mockPluginsManager = new Mock<IPluginsManager>();
			mockPluginsManager.Setup(_ => _.Start()).Throws(new Exception("Start failed"));

			var mockLogger = new Mock<ILogger<BillingService>>();
			mockLogger.Setup(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Failed to start billing service."), It.Is<Exception>(e => e.Message == "Start failed"), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
			mockLogger.Setup(_ => _.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			var mockBillingService = new Mock<BillingService>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), "plugins_settings.xml", "plugins_state.xml", loggerFactory) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(It.IsAny<string>(), It.IsAny<string>())).Returns(mockPluginsManager.Object);
			mockBillingService.Setup(_ => _.CreateLogger()).Returns(mockLogger.Object);
			mockBillingService.Setup(_ => _.GetErrorReportingClient()).Returns(mockErrorReportingClient.Object);

			Start(mockBillingService.Object);

			mockPluginsManager.Verify(_ => _.Start());
			mockPluginsManager.Verify(_ => _.Dispose());
			mockLogger.VerifyAll();
		}

		[Test]
		public void TestReadSettingsFile_PluginsElementIsEmpty_ShouldReportToIssueManager()
		{
			var stateFilePath = Path.GetTempFileName();
			var settingsFilePath = Path.GetTempFileName();

			var mockLogger = new Mock<ILogger<PluginsManager>>();
			mockLogger.Setup(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "No plugins found in the settings file."), It.Is<FormatException>(e => e.Message == $"No plugins found in the settings file: {settingsFilePath}"), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
			mockLogger.Setup(_ => _.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			var mockPluginsManager = new Mock<TestPluginManager>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"),
				settingsFilePath,
				stateFilePath,
				globalSettings,
				loggerFactory,
				serviceClientFactory,
				kafkaClientFactory,
				mockErrorReportingClient.Object,
				new TestClock(),
				null)
			{
				CallBase = true
			};
			mockPluginsManager.Setup(_ => _.CreateLogger()).Returns(mockLogger.Object);
			var mockBillingService = new Mock<BillingService>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), settingsFilePath, stateFilePath, loggerFactory) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(settingsFilePath, stateFilePath)).Returns(mockPluginsManager.Object);
			mockBillingService.Setup(_ => _.CreateLogger()).Returns(new Mock<ILogger<BillingService>>().Object);

			var now = new DateTime(2018, 2, 2, 2, 0, 0);
			var clock = new TestClock { UtcNow = now };
			var initialLastSuccessfulRun = now - TimeSpan.FromMinutes(10) - TimeSpan.FromSeconds(3);

			var state = new PluginControllerState(clock)
			{
				Key = "DummyBillingTransactionsPlugin",
				LastSuccessfulRun = initialLastSuccessfulRun
			};
			var serviceState = new ServiceState();
			serviceState.Plugins.Add(state);

			try
			{
				File.WriteAllText(settingsFilePath, @"<?xml version=""1.0""?>
<Plugins />");

				serviceState.WriteToFile(stateFilePath);

				Start(mockBillingService.Object);

				mockPluginsManager.Verify(_ => _.Start());
				mockPluginsManager.Verify(_ => _.Dispose(), Times.Never());
				mockLogger.VerifyAll();
			}
			finally
			{
				DummyBillingTransactionsPlugin.Reset();
				File.Delete(stateFilePath);
				File.Delete(settingsFilePath);
			}
		}

		[Test]
		public void TestReadStateFile_PluginsElementIsEmpty_ShouldReportToIssueManager()
		{
			var stateFilePath = Path.GetTempFileName();
			var settingsFilePath = Path.GetTempFileName();

			var mockLogger = new Mock<ILogger<PluginsManager>>();
			mockLogger.Setup(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "No plugins found in the state file."), It.Is<FormatException>(e => e.Message == $"No plugins found in the state file: {stateFilePath}"), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
			mockLogger.Setup(_ => _.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			var mockPluginsManager = new Mock<TestPluginManager>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"),
				settingsFilePath,
				stateFilePath,
				globalSettings,
				loggerFactory,
				serviceClientFactory,
				kafkaClientFactory,
				mockErrorReportingClient.Object,
				new TestClock(),
				null)
			{
				CallBase = true
			};
			mockPluginsManager.Setup(_ => _.CreateLogger()).Returns(mockLogger.Object);
			var mockBillingService = new Mock<BillingService>(Assembly.GetExecutingAssembly(), settingsFilePath, stateFilePath, loggerFactory) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(settingsFilePath, stateFilePath)).Returns(mockPluginsManager.Object);
			mockBillingService.Setup(_ => _.CreateLogger()).Returns(new Mock<ILogger<BillingService>>().Object);

			try
			{
				File.WriteAllText(stateFilePath, @"<?xml version=""1.0""?>
<Plugins />");

				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>1</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsFilePath, settingsXml);

				Start(mockBillingService.Object);

				mockPluginsManager.Verify(_ => _.Start());
				mockPluginsManager.Verify(_ => _.Dispose(), Times.Never());
				mockLogger.VerifyAll();
			}
			finally
			{
				DummyBillingTransactionsPlugin.Reset();
				File.Delete(stateFilePath);
				File.Delete(settingsFilePath);
			}
		}

		[Test]
		public void TestWriteToStateFile_FileIsReadOnly_ShouldReportToIssueManager()
		{
			var settingsFilePath = Path.GetTempFileName();
			var stateFilePath = Path.GetTempFileName();

			var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>600</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
			File.WriteAllText(settingsFilePath, settingsXml);

			var now = new DateTime(2018, 2, 2, 2, 0, 0);
			var clock = new TestClock { UtcNow = now };
			var initialLastSuccessfulRun = now - TimeSpan.FromMinutes(10) - TimeSpan.FromSeconds(3);

			var state = new PluginControllerState(clock)
			{
				Key = "DummyBillingTransactionsPlugin",
				LastSuccessfulRun = initialLastSuccessfulRun
			};
			var serviceState = new ServiceState();
			serviceState.Plugins.Add(state);
			serviceState.WriteToFile(stateFilePath);

			File.SetAttributes(stateFilePath, FileAttributes.ReadOnly);

			var mockLogger = new Mock<ILogger<PluginsManager>>();
			mockLogger.Setup(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Failed to write to state file"), It.Is<UnauthorizedAccessException>(s => s.Message == $"Access to the path '{stateFilePath}' is denied."), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
			mockLogger.Setup(_ => _.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((o, t) => o.ToString() == "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

			var mockPluginsManager = new Mock<TestPluginManager>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"),
				settingsFilePath,
				stateFilePath,
				globalSettings,
				loggerFactory,
				serviceClientFactory,
				kafkaClientFactory,
				mockErrorReportingClient.Object,
				clock,
				null)
			{
				CallBase = true
			};
			mockPluginsManager.Setup(_ => _.CreateLogger()).Returns(mockLogger.Object);

			var mockBillingService = new Mock<BillingService>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), settingsFilePath, stateFilePath, loggerFactory) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(settingsFilePath, stateFilePath)).Returns(mockPluginsManager.Object);
			mockBillingService.Setup(_ => _.GetErrorReportingClient()).Returns(mockErrorReportingClient.Object);
			mockBillingService.Setup(_ => _.CreateLogger()).Returns(new Mock<ILogger<BillingService>>().Object);

			try
			{
				Start(mockBillingService.Object);

				mockPluginsManager.Verify(_ => _.Start());
				mockPluginsManager.Verify(_ => _.Dispose(), Times.Never());
				mockPluginsManager.VerifyAll();
				mockLogger.VerifyAll();
			}
			finally
			{
				DummyBillingTransactionsPlugin.Reset();
				File.SetAttributes(stateFilePath, FileAttributes.Normal);
				File.Delete(stateFilePath);
				File.Delete(settingsFilePath);
			}
		}

		[Test]
		public void TestStartAndLoadFailure()
		{
			var mockPluginsManager = new Mock<IPluginsManager>();
			mockPluginsManager.Setup(_ => _.Start()).Raises(_ => _.PluginsLoadFailed += null, EventArgs.Empty);
			var mockBillingService = new Mock<BillingService>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), "plugins_settings.xml", "plugins_state.xml", loggerFactory) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(It.IsAny<string>(), It.IsAny<string>())).Returns(mockPluginsManager.Object);

			Start(mockBillingService.Object);

			mockPluginsManager.Verify(_ => _.Start());
			mockPluginsManager.Verify(_ => _.Dispose());
		}

		[TestCaseSource(nameof(SqlErrors))]
		public void TestTransactionPluginLogger_SqlException(string error)
		{
			var settingsFilePath = Path.GetTempFileName();
			var stateFilePath = Path.GetTempFileName();

			var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>10</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
			File.WriteAllText(settingsFilePath, settingsXml);

			var now = DateTime.UtcNow;
			var clock = new TestClock { UtcNow = now };
			var lastSuccessfulRun = now.AddMinutes(-11);
			var lastTransactionTimestamp = now.AddMinutes(-16);

			var state = new PluginControllerState(clock)
			{
				Key = "DummyBillingTransactionsPlugin",
				LastSuccessfulRun = lastSuccessfulRun,
				LastTransactionTimestamp = lastTransactionTimestamp
			};
			var serviceState = new ServiceState();
			serviceState.Plugins.Add(state);
			serviceState.WriteToFile(stateFilePath);

			File.SetAttributes(stateFilePath, FileAttributes.ReadOnly);
			
			var billingTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = now.AddMinutes(-6),
				ReportingSource = "TST"
			};
			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				new TimeStampedTransaction(now.AddMinutes(-5), billingTransaction)
			};

			SqlException sqlEx = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SqlException)) as SqlException;
			typeof(SqlException).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sqlEx, error);
			mockServiceClient.Setup(_ => _.AddTransaction(billingTransaction)).Throws(sqlEx);

			var mockPluginsManager = new Mock<TestPluginManager>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"),
				settingsFilePath,
				stateFilePath,
				globalSettings,
				loggerFactory,
				serviceClientFactory,
				kafkaClientFactory,
				mockErrorReportingClient.Object,
				clock,
				BillingService.KnownExceptions)
			{
				CallBase = true
			};

			var mockBillingService = new Mock<BillingService>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), settingsFilePath, stateFilePath, loggerFactory) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(settingsFilePath, stateFilePath)).Returns(mockPluginsManager.Object);
			mockBillingService.Setup(_ => _.GetErrorReportingClient()).Returns(mockErrorReportingClient.Object);
			mockBillingService.Setup(_ => _.CreateLogger()).Returns(new Mock<ILogger<BillingService>>().Object);

			try
			{
				Start(mockBillingService.Object);

				mockPluginsManager.Verify(_ => _.Start());
				mockPluginsManager.Verify(_ => _.Dispose(), Times.Never());
				mockPluginsManager.VerifyAll();

				string warnLogMessage = string.Empty;

				if (error == "Test Error")
				{
					memoryAppender = GetMemoryAppender();
					var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).FirstOrDefault();
					Assert.AreEqual("Error happened while collecting and sending transactions.", errorLogMessage.MessageObject.ToString());
					warnLogMessage = "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'";
				}
				else
				{
					memoryAppender = GetMemoryAppender();
					var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).FirstOrDefault();
					Assert.IsNull(errorLogMessage);
					warnLogMessage = "Error happened while collecting and sending transactions.";
				}

				var warningLogMessagesList = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).ToList();
				Assert.That(warningLogMessagesList.Count, Is.EqualTo(1));
				Assert.That(warningLogMessagesList[0].MessageObject.ToString(), Does.StartWith(warnLogMessage));

				mockServiceClient.VerifyAll();
			}
			finally
			{
				DummyBillingTransactionsPlugin.Reset();
				File.SetAttributes(stateFilePath, FileAttributes.Normal);
				File.Delete(stateFilePath);
				File.Delete(settingsFilePath);
			}
		}

		[TestCaseSource(nameof(FaultExceptions))]
		public void TestTransactionPluginLogger_FaultException(string error)
		{
			var settingsFilePath = Path.GetTempFileName();
			var stateFilePath = Path.GetTempFileName();

			var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>10</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
			File.WriteAllText(settingsFilePath, settingsXml);

			var now = DateTime.UtcNow;
			var clock = new TestClock { UtcNow = now };
			var lastSuccessfulRun = now.AddMinutes(-11);
			var lastTransactionTimestamp = now.AddMinutes(-16);

			var state = new PluginControllerState(clock)
			{
				Key = "DummyBillingTransactionsPlugin",
				LastSuccessfulRun = lastSuccessfulRun,
				LastTransactionTimestamp = lastTransactionTimestamp
			};
			var serviceState = new ServiceState();
			serviceState.Plugins.Add(state);
			serviceState.WriteToFile(stateFilePath);

			File.SetAttributes(stateFilePath, FileAttributes.ReadOnly);

			var billingTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = now.AddMinutes(-6),
				ReportingSource = "TST"
			};
			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				new TimeStampedTransaction(now.AddMinutes(-5), billingTransaction)
			};

			FaultException faultEx = new FaultException(error);
			mockServiceClient.Setup(_ => _.AddTransaction(billingTransaction)).Throws(faultEx);

			var mockPluginsManager = new Mock<TestPluginManager>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"),
				settingsFilePath,
				stateFilePath,
				globalSettings,
				loggerFactory,
				serviceClientFactory,
				kafkaClientFactory,
				mockErrorReportingClient.Object,
				clock,
				BillingService.KnownExceptions)
			{
				CallBase = true
			};

			var mockBillingService = new Mock<BillingService>(Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), settingsFilePath, stateFilePath, loggerFactory) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(settingsFilePath, stateFilePath)).Returns(mockPluginsManager.Object);
			mockBillingService.Setup(_ => _.GetErrorReportingClient()).Returns(mockErrorReportingClient.Object);
			mockBillingService.Setup(_ => _.CreateLogger()).Returns(new Mock<ILogger<BillingService>>().Object);

			try
			{
				Start(mockBillingService.Object);

				mockPluginsManager.Verify(_ => _.Start());
				mockPluginsManager.Verify(_ => _.Dispose(), Times.Never());
				mockPluginsManager.VerifyAll();

				string warnLogMessage = string.Empty;
				memoryAppender = GetMemoryAppender();

				if (error == "Other Error")
				{
					var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).FirstOrDefault();
					Assert.AreEqual("Error happened while collecting and sending transactions.", errorLogMessage.MessageObject.ToString());
					warnLogMessage = "Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'";
				}
				else
				{
					var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).FirstOrDefault();
					Assert.IsNull(errorLogMessage);
					warnLogMessage = "Error happened while collecting and sending transactions.";
				}

				var warningLogMessagesList = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).ToList();
				Assert.That(warningLogMessagesList.Count, Is.EqualTo(1));
				Assert.That(warningLogMessagesList[0].MessageObject.ToString(), Does.StartWith(warnLogMessage));

				mockServiceClient.VerifyAll();
			}
			finally
			{
				DummyBillingTransactionsPlugin.Reset();
				File.SetAttributes(stateFilePath, FileAttributes.Normal);
				File.Delete(stateFilePath);
				File.Delete(settingsFilePath);
			}
		}

		[SetUp]
		public void Setup()
		{
			memoryAppender = new MemoryAppender();
			BasicConfigurator.Configure(memoryAppender);
			mockServiceClient = new Mock<IBillingServiceClient>();
			mockKafkaClient = new Mock<IBillingKafkaClient>();
			mockErrorReportingClient = new Mock<IErrorReportingClient>();
			mockErrorReportingClient.Setup(x => x.ServiceUri).Returns(new Uri("", UriKind.Relative));
			loggerFactory = new LoggerFactory().AddLog4Net(new Log4NetProviderOptions("SqlBillingTransactionsPluginTest.log4net.config"));
			serviceClientFactory = () => mockServiceClient.Object;
			kafkaClientFactory = () => mockKafkaClient.Object;
		}

		static void Start(BillingService service)
		{
			var onStart = typeof(BillingService).GetMethod("OnStart", BindingFlags.NonPublic | BindingFlags.Instance);
			onStart.Invoke(service, new object[] { null });
		}

		static MemoryAppender GetMemoryAppender() =>
			log4net.LogManager.GetRepository().GetAppenders().OfType<MemoryAppender>().Single();

		static string[] SqlErrors = { "Named Pipes Provider: Could not open a connection to SQL Server [].", "Test Error", "Could not use view or function 'eHubArchiveMessage' because of binding errors" };
		static string[] FaultExceptions = { "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible.", "Cannot open database \"billing\" requested by the login. The login failed.\r\nLogin failed for user 'billing_admin'", "Other Error" };

		Dictionary<string, string> globalSettings = new Dictionary<string, string>()
		{
			{ "UsageELKKafkaTopic", "TestTopic" },
			{ "IssueManagerUri", ""}
		};

		Mock<IBillingServiceClient> mockServiceClient;
		Mock<IBillingKafkaClient> mockKafkaClient;
		Mock<IErrorReportingClient> mockErrorReportingClient;
		ILoggerFactory loggerFactory;
		Func<IBillingServiceClient> serviceClientFactory;
		Func<IBillingKafkaClient> kafkaClientFactory;
		MemoryAppender memoryAppender;
	}
}
