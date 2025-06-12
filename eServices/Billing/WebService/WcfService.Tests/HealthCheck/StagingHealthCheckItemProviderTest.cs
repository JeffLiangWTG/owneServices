using NUnit.Framework;
using Moq;
using System;
using CargoWise.eServices.Billing.DataAccess;
using System.Threading.Tasks;
using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class StagingHealthCheckItemProviderTests
	{
		Mock<IBillingRepository> mockRepository;
		Mock<StagingHealthCheckItemProvider> mockProvider;

		[SetUp]
		public void SetUp()
		{
			mockRepository = new Mock<IBillingRepository>();
			mockProvider = new Mock<StagingHealthCheckItemProvider>();
			mockProvider.Setup(_=>_.BillingRepository).Returns(mockRepository.Object);
		}

		[Test]
		public async Task TestCheckHealthAsync_Healthy_WhenStagingIsEmpty()
		{
			mockRepository.Setup(x => x.OldestSystemCreateUTCInStaging()).Returns((DateTime?)null);

			var result = await mockProvider.Object.CheckHealthAsync();

			Assert.AreEqual(HealthCheckStatus.OK, result.Status);
			Assert.AreEqual("Healthy. Staging is empty.", result.Description);
		}

		[Test]
		public async Task TestCheckHealthAsync_Healthy_WhenStagingIsNotBehind()
		{
			var utcNow = new DateTime(2024, 9, 10, 16, 00, 00);
			var oldestTime = utcNow.AddMinutes(-119);
			mockRepository.Setup(x => x.OldestSystemCreateUTCInStaging()).Returns(oldestTime);
			mockProvider.Setup(x => x.UtcNow()).Returns(utcNow);

			var result = await mockProvider.Object.CheckHealthAsync();

			Assert.AreEqual(HealthCheckStatus.OK, result.Status);
			Assert.AreEqual("Healthy. The time difference in hours '1.98' between the oldest Staging record created time UTC '10-Sep-2024 02:01:00 PM' and check time UTC '10-Sep-2024 04:00:00 PM' is less than 2.", result.Description);
		}

		[Test]
		public async Task TestCheckHealthAsync_Unhealthy_WhenStagingBehind()
		{
			var utcNow = new DateTime(2024, 9, 10, 16, 00, 00);
			var oldestTime = utcNow.AddMinutes(-121);
			mockRepository.Setup(x => x.OldestSystemCreateUTCInStaging()).Returns(oldestTime);
			mockProvider.Setup(x => x.UtcNow()).Returns(utcNow);

			var result = await mockProvider.Object.CheckHealthAsync();

			Assert.AreEqual(HealthCheckStatus.Error, result.Status);
			Assert.AreEqual("Staging is behind by more than 2 hours. The time difference in hours '2.02' between the oldest Staging record created time UTC '10-Sep-2024 01:59:00 PM' and check time UTC '10-Sep-2024 04:00:00 PM' is greater than or equal to 2.", result.Description);
		}

		[Test]
		public async Task TestCheckHealthAsync_Unhealthy_WhenExceptionIsThrown()
		{
			mockRepository.Setup(x => x.OldestSystemCreateUTCInStaging()).Throws(new Exception("Database connection error."));

			var result = await mockProvider.Object.CheckHealthAsync();

			Assert.AreEqual(HealthCheckStatus.Error, result.Status);
			Assert.AreEqual("An exception 'System.Exception' was thrown during health check. Message: Database connection error.", result.Description);
		}
	}
}
