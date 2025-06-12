using CargoWise.eServices.Billing.DataAccess;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using CargoWise.eServices.Monitoring.HealthCheck.API;
	using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
	using Moq;
	using NUnit.Framework;

	class ELKResubmissionHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<ELKResubmissionHealthCheckItemProvider>
	{
		[TestCase(true, HealthCheckStatus.Error, "Backlog has exceeded its threshold (500)", TestName = "TestELKResubmissionHealthCheckItemProvider_Error")]
		[TestCase(false, HealthCheckStatus.OK, "Healthy", TestName = "TestELKResubmissionHealthCheckItemProvider_Healthy")]
		public async Task TestELKResubmissionHealthCheckItemProvider(bool exceedingThreshold, HealthCheckStatus expectedStatus, string expectedDescription)
		{
			var mockRepo = new Mock<IBillingRepository>();
			mockRepo.Setup(x => x.DoesELKBacklogExceedThreshold(It.IsAny<int>())).Returns(exceedingThreshold);
			var mockProvider = new Mock<ELKResubmissionHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(x => x.CreateRepository()).Returns(mockRepo.Object);
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("ELKResubmission"));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(expectedStatus));
			Assert.That(checkItem.Description, Is.EqualTo(expectedDescription));
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();

			var mockRepo = new Mock<IBillingRepository>();
			mockRepo.Setup(x => x.DoesELKBacklogExceedThreshold(It.IsAny<int>())).Returns(false);

			var mockProvider = new Mock<ELKResubmissionHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			var provider = mockProvider.Object;

			mockProvider.Setup(x => x.CreateRepository()).Returns(mockRepo.Object);
			var checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			return descriptions;
		}
	}
}
