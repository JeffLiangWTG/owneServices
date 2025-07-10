using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WtgStatusHealthCheckFixture
	{
		[Test]
		public async Task CheckHealthAsync()
		{
			var log = new Mock<ILog>();
			var logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<WtgStatusHealthCheck>()).Returns(log.Object);
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			var healthCheck = new WtgStatusHealthCheck(logWrapper.Object, connectionString);
			var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());
			Assert.AreEqual(HealthStatus.Healthy, result.Status);
			Assert.AreEqual("Service is OK", result.Description);
			log.Verify(x => x.Info("Service is OK"));

			healthCheck = new WtgStatusHealthCheck(logWrapper.Object, connectionString.Replace(dbName, "Test_RefDbRepo_NotExist", StringComparison.OrdinalIgnoreCase));
			result = await healthCheck.CheckHealthAsync(new HealthCheckContext());
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
			Assert.True(result.Description.Contains("login failed", StringComparison.OrdinalIgnoreCase));
			log.Verify(x => x.Error(It.IsAny<Exception>()));
		}
	}
}
