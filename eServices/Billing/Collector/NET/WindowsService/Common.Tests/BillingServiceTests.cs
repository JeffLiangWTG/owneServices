using System.Reflection;
using CargoWise.Billing.Client;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.Billing.CollectorService.Plugin.Tests;
using CargoWise.Billing.Kafka.API;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common.Tests
{
	[TestFixture]
	public class BillingServiceTests
	{
		[Test]
		public void TestStartAndStop()
		{
			var settingsFileName = "plugins_settings.xml";
			var stateFileName = "plugins_state.xml";
			var settingsFilePath = Path.Combine(Path.GetDirectoryName(typeof(BillingService).Assembly.Location), settingsFileName);
			var stateFilePath = Path.Combine(Path.GetDirectoryName(typeof(BillingService).Assembly.Location), stateFileName);

			var mockPluginsManager = new Mock<IPluginsManager>();
			var mockBillingService = new Mock<BillingService>(mockServices.Object, Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), settingsFileName, stateFileName) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(settingsFilePath, stateFilePath)).Returns(mockPluginsManager.Object);

			var billingService = mockBillingService.Object;
			Start(billingService);

			mockPluginsManager.Verify(_ => _.Start());
			mockPluginsManager.Verify(_ => _.Dispose(), Times.Never);

			billingService.StopAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

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

			var mockBillingService = new Mock<BillingService>(mockServices.Object, Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), "plugins_settings.xml", "plugins_state.xml") { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(It.IsAny<string>(), It.IsAny<string>())).Returns(mockPluginsManager.Object);
			mockBillingService.Setup(_ => _.CreateLogger()).Returns(mockLogger.Object);

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
				mockLoggerFactory.Object,
				serviceClientFactory,
				kafkaClientFactory,
				mockErrorReportingClient.Object,
				new TestClock(),
				null)
			{
				CallBase = true
			};
			mockPluginsManager.Setup(_ => _.CreateLogger()).Returns(mockLogger.Object);
			var mockBillingService = new Mock<BillingService>(mockServices.Object, Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), settingsFilePath, stateFilePath) { CallBase = true };
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
				mockLoggerFactory.Object,
				serviceClientFactory,
				kafkaClientFactory,
				mockErrorReportingClient.Object,
				new TestClock(),
				null)
			{
				CallBase = true
			};
			mockPluginsManager.Setup(_ => _.CreateLogger()).Returns(mockLogger.Object);
			var mockBillingService = new Mock<BillingService>(mockServices.Object, Assembly.GetExecutingAssembly(), settingsFilePath, stateFilePath) { CallBase = true };
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
				mockLoggerFactory.Object,
				serviceClientFactory,
				kafkaClientFactory,
				mockErrorReportingClient.Object,
				clock,
				null)
			{
				CallBase = true
			};
			mockPluginsManager.Setup(_ => _.CreateLogger()).Returns(mockLogger.Object);

			var mockBillingService = new Mock<BillingService>(mockServices.Object, Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), settingsFilePath, stateFilePath) { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(settingsFilePath, stateFilePath)).Returns(mockPluginsManager.Object);
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
			var mockBillingService = new Mock<BillingService>(mockServices.Object, Assembly.LoadFrom("CargoWise.Billing.CollectorService.Plugin.Tests.dll"), "plugins_settings.xml", "plugins_state.xml") { CallBase = true };
			mockBillingService.Setup(_ => _.CreatePluginsManager(It.IsAny<string>(), It.IsAny<string>())).Returns(mockPluginsManager.Object);

			Start(mockBillingService.Object);

			mockPluginsManager.Verify(_ => _.Start());
			mockPluginsManager.Verify(_ => _.Dispose());
		}

		[SetUp]
		public void Setup()
		{
			mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
			mockConfiguration.Setup(_ => _["IssueManagerUri"]).Returns(string.Empty);
			mockServices = new Mock<IServiceProvider>();
			mockServiceClient = new Mock<IBillingServiceClient>();
			mockKafkaClient = new Mock<IBillingKafkaClient>();
			mockErrorReportingClient = new Mock<IErrorReportingClient>();
			mockErrorReportingClient.Setup(x => x.ServiceUri).Returns(new Uri("", UriKind.Relative));
			mockLogger = new Mock<ILogger>();
			mockLoggerFactory = new Mock<ILoggerFactory>();
			mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
			mockServices.Setup(_ => _.GetService(It.Is<Type>(t => t == typeof(IBillingServiceClient)))).Returns(mockServiceClient.Object);
			mockServices.Setup(_ => _.GetService(It.Is<Type>(t => t == typeof(IBillingKafkaClient)))).Returns(mockKafkaClient.Object);
			mockServices.Setup(_ => _.GetService(It.Is<Type>(t => t == typeof(ILoggerFactory)))).Returns(mockLoggerFactory.Object);
			mockServices.Setup(_ => _.GetService(It.Is<Type>(t => t == typeof(Microsoft.Extensions.Configuration.IConfiguration)))).Returns(mockConfiguration.Object);
			mockServices.Setup(_ => _.GetService(It.Is<Type>(t => t == typeof(IErrorReportingClient)))).Returns(mockErrorReportingClient.Object);
			serviceClientFactory = () => mockServices.Object.GetService<IBillingServiceClient>();
			kafkaClientFactory = () => mockServices.Object.GetService<IBillingKafkaClient>();
		}

		static void Start(BillingService service) => service.StartAsync(CancellationToken.None).Wait();

		Dictionary<string, string> globalSettings = new ()
		{
			{ "UsageELKKafkaTopic", "TestTopic" },
			{ "IssueManagerUri", ""}
		};

		Mock<IBillingServiceClient> mockServiceClient;
		Mock<IBillingKafkaClient> mockKafkaClient;
		Mock<IErrorReportingClient> mockErrorReportingClient;
		Mock<ILoggerFactory> mockLoggerFactory;
		Mock<ILogger> mockLogger;
		Mock<Microsoft.Extensions.Configuration.IConfiguration> mockConfiguration;
		Mock<IServiceProvider> mockServices;
		Func<IBillingServiceClient> serviceClientFactory;
		Func<IBillingKafkaClient> kafkaClientFactory;
	}
}
