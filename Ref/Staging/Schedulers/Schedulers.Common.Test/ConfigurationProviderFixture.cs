using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common.Test
{
	[TestFixture]
	public class ConfigurationProviderFixture
	{
		[Test]
		public void ValidateConfiguration()
		{
			var configuration = ConfigurationProvider.Configuration;
			Assert.IsNotNull(configuration);
			var quartzProps = ConfigurationProvider.QuartzProps;
			Assert.IsNotNull(quartzProps);
			Assert.AreEqual(quartz.Count, quartzProps.Count);
			foreach (var item in quartz)
			{
				Assert.AreEqual(item.Value, quartzProps[item.Key]);
			}

			Assert.AreEqual("9000", ConfigurationProvider.QuartzPanelPort);
			Assert.AreEqual("RefDbRepoQuartzServer", ConfigurationProvider.SchedulerName);
			Assert.AreEqual("http://localhost:37016/odata", ConfigurationProvider.SafeUpdateServiceUri);
			Assert.AreEqual("data source=localhost;initial catalog=RefDbRepoStaging;integrated security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;", ConfigurationProvider.StagingConnectionString);
			Assert.AreEqual(false, ConfigurationProvider.CheckAuthorization);
			Assert.That(ConfigurationProvider.MemoryMonitorIntervalInSeconds, Is.EqualTo(5));
		}

		static readonly Dictionary<string, string> quartz = new Dictionary<string, string>
		{
			{"quartz.scheduler.instanceName", "RefDbRepoQuartzServer"},
			{"quartz.scheduler.instanceId", "auto"},
			{"quartz.threadPool.maxConcurrency", "10"},
			{"quartz.jobStore.misfireThreshold", "60000"},
			{"quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"},
			{"quartz.jobStore.useProperties", "false"},
			{"quartz.jobStore.dataSource", "default"},
			{"quartz.jobStore.tablePrefix", "QRTZ_"},
			{"quartz.jobStore.clustered", "true"},
			{"quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz"},
			{"quartz.dataSource.default.connectionString", "data source=localhost;initial catalog=RefDbRepoStaging;integrated security=True;TrustServerCertificate=True"},
			{"quartz.dataSource.default.provider", "SqlServer"},
			{"quartz.plugin.jobInitializer.type", "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz.Plugins"},
			{"quartz.plugin.jobInitializer.fileNames", "~\\Configuration\\QuartzAppRunnerJobs.xml,~\\Configuration\\QuartzProcessorRunnerJobs.xml,~\\Configuration\\SingleInstanceQuartzProcessorRunnerJobs.xml,~\\Configuration\\QuartzDataUpdaterJobs.xml" },
			{"quartz.plugin.jobInitializer.failOnFileNotFound", "true"},
			{"quartz.server.serviceName", "RefDbRepoQuartzServer"},
			{"quartz.server.serviceDisplayName", "RefDbRepo Quartz Server"},
			{"quartz.server.serviceDescription", "RefDbRepo Quartz Job Scheduling Server"},
			{"quartz.serializer.type", "json" }
		};
	}
}
