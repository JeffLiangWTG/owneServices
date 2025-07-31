using System.Reflection;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.MSearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Serilog;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Tests
{
	[TestFixture]
	class WiseCloudReadOnlyPluginTest
	{
		private Mock<ElasticsearchClient> mockClient;
		private PluginSettings _settings = new PluginSettings();
		private Plugins.WiseCloudReadOnly.Plugin plugin;
		Mock<ILoggerFactory> mockLoggerFactory;
		Mock<Microsoft.Extensions.Logging.ILogger> mockLogger;
		private Dictionary<string, string[]> testDefectiveQueryHashesDictionary;
		private string enterpriseCode;
		private string serverCode;

		[SetUp]
		public void Setup()
		{
			mockClient = new Mock<ElasticsearchClient>();
			plugin = new  Plugins.WiseCloudReadOnly.Plugin();
			mockLoggerFactory = new Mock<ILoggerFactory>();
			mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger>();
			mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
			plugin.SetLoggerFactory(mockLoggerFactory.Object);
			enterpriseCode = "XYZ";
			serverCode = "789";
		}

		[Test]
		public void TestUpdateSettings()
		{

			_settings = new PluginSettings()
			{
				Parameters = new PluginParameter[]
				{
					new PluginParameter("ElasticEndpoint", "https://r.test-1.es.wtg.ws:443"),
					new PluginParameter("ElasticApiKey", "k++HhEyGTc2m1oj0+YUCAGzS43thuQnQDJqwNkeCH45ceU8sGwy+6ZzpSAwk/i8mLDMJBJcZd9feTPjq3dJhYW8wI5x0ocuuWlFwa1Vo8UG8rGHwszb/6foHBxPoJ8Wm2COM/0IO8bW4P6JKy+DN5PNFlU8fb29asLNsRmu0U4s="),
					new PluginParameter("HAProxyIndex", "logs-haproxy.logfile-wtg"),
					new PluginParameter("ReferenceFilePath", "\\\\sydco-scfs-1.wtg.zone\\globaldata\\InformationServices\\SQLReadOnlyReferenceFile\\WiseCloudSQLAccessReferenceFile.csv"),
					new PluginParameter("ReferenceFileServerUsername", "s_billingservice"),
					new PluginParameter("ReferenceFileServerPassword", "password"),
					new PluginParameter("ReferenceFileServerDomainName", "CORP"),
					new PluginParameter("BatchSize", "20"),
					new PluginParameter("ElasticRetryMaxAttempts", "1440"),
					new PluginParameter("ElasticRetryDelayInSecond", "60")
				}
			};
			plugin.UpdateSettings(_settings);
			Assert.That(plugin.ElasticEndpoint, Is.EqualTo("https://r.test-1.es.wtg.ws:443"));
			Assert.That(plugin.HAProxyIndex, Is.EqualTo("logs-haproxy.logfile-wtg"));
			Assert.That(plugin.BatchSize, Is.EqualTo(20));
			Assert.That(plugin.ElasticRetryMaxAttempts, Is.EqualTo(1440));
			Assert.That(plugin.ElasticRetryDelayInSecond, Is.EqualTo(60));
		}
		[Test]
		public void TestGetTransaction()
		{
			string defectiveQueryHashesJson = JsonConvert.SerializeObject(testDefectiveQueryHashesDictionary);
			//var testSettings = new PluginSettings()
			//{
			//	Parameters = new PluginParameter[]
			//	{
			//		new PluginParameter("ElasticEndpoint", "https://elastic.apac-test-1.sand.wtg.zone:443"),
			//		new PluginParameter("ElasticApiKey", "Q9gRykeosgARqlZ/aNme9Qvaog6q00mWF4WTJxO88S1v8LW5cgbpMXtJ/uMSO0KnWDcNogPzfh1c01cWym/AhSyawSay9xq/cFwuVJ8Ow0shKXcIsgItfYZJfwiw/RWjG7AW6387CT56lruSYxdoybNvuG775zYjZz3OSij81lE="),
			//		new PluginParameter("CpuMonitoringIndex", "logs-haproxy.logfile-wtg"),
			//		new PluginParameter("BatchSize", "20"),
			//		new PluginParameter("ElasticRetryMaxAttempts", "1440"),
			//		new PluginParameter("ElasticRetryDelayInSecond", "60")
			//	}
			//};

			//var prodAPACsettings = new PluginSettings()
			//{
			//	Parameters = new PluginParameter[]
			//	{
			//		new PluginParameter("ElasticEndpoint", "https://elastic.apac-prod-1.wtg.zone:443"),
			//		new PluginParameter("ElasticApiKey", "wjXkbHWQpFkYmIBmZXDxalsz6vpom6n+5BZr/eC8uwrY+0CXuTmRPLKJblBa/lAfvEI/2Idz9jjeot7eoxbEWer3N4NFvldsZxrwp+f6gkwE3yL98O3+FGm/8ttfyH6bm9BcuuYA/T5t8o9bbzQwKjvTAdu3Txu+7rsbFAGeJAc="),
			//		new PluginParameter("CpuMonitoringIndex", "logs-haproxy.logfile-wtg"),
			//		new PluginParameter("ReferenceFilePath", Path.Combine(TestContext.CurrentContext.TestDirectory,"WiseCloudReadOnlyPluginTest", "TestData", "test_reference.csv")),
			//		new PluginParameter("BatchSize", "20"),
			//		new PluginParameter("DefectiveQueryHashesDictionary", defectiveQueryHashesJson),
			//		new PluginParameter("ElasticRetryMaxAttempts", "1440"),
			//		new PluginParameter("ElasticRetryDelayInSecond", "60")
			//	}
			//};

			//var prodEMEAsettings = new PluginSettings()
			//{
			//	Parameters = new PluginParameter[]
			//	{
			//		new PluginParameter("ElasticEndpoint", "https://elastic.emea-prod-1.wtg.zone:443"),
			//		new PluginParameter("ElasticApiKey", "o2teZVJ8PDdYsTDmUr+1FnvMyF1sn74p4aC1d7POeY/OTxB1s1WIPmpz/bA0bCugZrc5jUnbAURHpeZTLN9eZyrVmX+MnP2TCEFXbbk3stjnX4YigcaFLrTj+ijLfInNYQZNsyStPd9SgpgXqiLEAx1XpuRBdrI8hvdxLbEPqTI="),
			//		new PluginParameter("HAProxyIndex", "logs-haproxy.logfile-wtg"),
			//		new PluginParameter("ReferenceFilePath", Path.Combine(TestContext.CurrentContext.TestDirectory,"WiseCloudReadOnlyPluginTest", "TestData", "test_reference.csv")),
			//		new PluginParameter("BatchSize", "20"),
			//		new PluginParameter("DefectiveQueryHashesDictionary", defectiveQueryHashesJson),
			//		new PluginParameter("ElasticRetryMaxAttempts", "1440"),
			//		new PluginParameter("ElasticRetryDelayInSecond", "60")
			//	}
			//};

			var prodAMERsettings = new PluginSettings()
			{
				Parameters = new PluginParameter[]
				{
					new PluginParameter("ElasticEndpoint", "https://elastic.amer-prod-1.wtg.zone:443"),
					new PluginParameter("ElasticApiKey", "kR8pjMk233CSQinY7pWOKuB5LkR8gt0IzvTSe5UWwFim2res03SWd+sRMgx3yGeY9hxHbFmJWtHQlEVluhpCvLkzI8YsCRrcG3ysCtbSGR7CTFWhXZLa3tFhjrbE8VwD6BdgwLChH8nLs/kV06OJ49c/u5SaN8y9RInHOp3aO1g="),
					new PluginParameter("HAProxyIndex", "logs-haproxy.logfile-wtg"),
					new PluginParameter("ReferenceFilePath", Path.Combine(TestContext.CurrentContext.TestDirectory,"WiseCloudReadOnlyPluginTest", "TestData", "test_reference.csv")),
					new PluginParameter("BatchSize", "20"),
					new PluginParameter("DefectiveQueryHashesDictionary", defectiveQueryHashesJson),
					new PluginParameter("ElasticRetryMaxAttempts", "1440"),
					new PluginParameter("ElasticRetryDelayInSecond", "60")
				}
			};

			var plugin = new Plugins.WiseCloudReadOnly.Plugin();
			plugin.UpdateSettings(prodAMERsettings);
			var logger = new LoggerConfiguration().WriteTo.File("C:\\TestElasticSearchSearchWiseCloud.log", rollOnFileSizeLimit: true, retainedFileCountLimit: 10).CreateLogger();
			plugin.SetLoggerFactory(new LoggerFactory().AddSerilog(logger));
			var start = new DateTime(2025, 7, 30, 00, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2025, 7, 30, 00, 5, 0, DateTimeKind.Utc);
			//var timeStampedTransactions = plugin.GetTransactions(start, end).ToList();
			var timeStampedTransactions = plugin.GetTransactionInPage(start, end).ToList();
		}

		[Test]
		public void ProcessReferenceFile_LoadsMappings()
		{
			var path = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				"WiseCloudReadOnlyPluginTest",
				"TestData",
				"test_reference.csv"
			);
			var logger = NullLoggerFactory.Instance.CreateLogger("test");
			var mapping = ReferenceFileProcessor.ProcessReferenceFile(path, logger);
			Assert.That(mapping["207.170.227.194"], Is.EqualTo("RGLBVE"),  "IP should map correctly");
			Assert.That(mapping["204.228.134.17"], Is.EqualTo("A21SLC"),  "Ranged IP should map correctly");
			Assert.That(mapping.ContainsKey("3.214.190.16"), Is.False, "Duplicated Record should not exist");
		}
	}
}
