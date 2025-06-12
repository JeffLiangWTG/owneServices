using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.Billing.Kafka.API;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase;
using CargoWise.eServices.Billing.WcfService.HealthCheck;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
using Castle.Windsor;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	public class ProcessingBillingDatabaseHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<ProcessingBillingDatabaseHealthCheckItemProvider>
	{
		[Test]
		public async Task TestProcessingBillingDatabaseHealthCheckItemProvider()
		{
			succeededJobList.Add(new KeyValuePair<string, SucceededJobDto>("1", new SucceededJobDto
			{
				Job = new Job(typeof(BillingDatabaseProcessor), typeof(BillingDatabaseProcessor).GetMethod("RecurringProcess"), new object[] { null }),
				SucceededAt = DateTime.SpecifyKind(new DateTime(2024, 4, 10, 1, 0, 1), DateTimeKind.Unspecified)
			}));
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("ProcessingBillingDatabase"));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
			Assert.That(checkItem.Description, Is.EqualTo("Healthy"));
		}

		[Test]
		public async Task TestProcessingBillingDatabaseHealthCheckItemProvider_NotSucceededInLastHour()
		{
			succeededJobList.Add(new KeyValuePair<string, SucceededJobDto>("1", new SucceededJobDto
			{
				Job = new Job(typeof(BillingDatabaseProcessor), typeof(BillingDatabaseProcessor).GetMethod("RecurringProcess"), new object[] { null }),
				SucceededAt = DateTime.SpecifyKind(new DateTime(2024, 4, 9, 23, 59, 59), DateTimeKind.Unspecified)
			}));
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("ProcessingBillingDatabase"));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Does.StartWith("Processing billing datbase did not succeed."));
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();

			var checkItem = mockProvider.Object.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			succeededJobList.Add(new KeyValuePair<string, SucceededJobDto>("1", new SucceededJobDto
			{
				Job = new Job(typeof(BillingDatabaseProcessor), typeof(BillingDatabaseProcessor).GetMethod("RecurringProcess"), new object[] { null }),
				SucceededAt = DateTime.SpecifyKind(new DateTime(2024, 4, 10, 1, 0, 1), DateTimeKind.Unspecified)
			}));
			checkItem = mockProvider.Object.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			return descriptions;
		}

		[SetUp]
		public void Setup()
		{
			dtProviderMock = new Mock<IDateTimeProvider>();
			dtProviderMock.Setup(x => x.DateTimeNow)
				.Returns(DateTime.SpecifyKind(new DateTime(2024, 4, 10, 11, 0, 0), DateTimeKind.Local));
			succeededJobList = new List<KeyValuePair<string, SucceededJobDto>>();
			monitoringApiMock = new Mock<IMonitoringApi>();
			monitoringApiMock.Setup(x => x.SucceededListCount()).Returns(() => succeededJobList.Count);
			monitoringApiMock.Setup(x => x.SucceededJobs(0, It.Is<int>(count => count == succeededJobList.Count))).Returns(() => new JobList<SucceededJobDto>(succeededJobList));
			containerMock = new Mock<IWindsorContainer>();
			containerMock.Setup(x => x.Resolve<IMonitoringApi>()).Returns(monitoringApiMock.Object);
			Global.WindsorContainer = containerMock.Object;
			mockProvider = new Mock<ProcessingBillingDatabaseHealthCheckItemProvider> { CallBase = true };
			mockProvider.Setup(x => x.DateTimeProvider).Returns(dtProviderMock.Object);
		}

		Mock<IMonitoringApi> monitoringApiMock;
		List<KeyValuePair<string, SucceededJobDto>> succeededJobList;
		Mock<IWindsorContainer> containerMock;
		Mock<IDateTimeProvider> dtProviderMock;
		Mock<ProcessingBillingDatabaseHealthCheckItemProvider> mockProvider;
	}
}
