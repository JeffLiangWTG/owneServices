using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.NewSchedulers.Test.IntegrationTest;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[TransactionedTestCase]
	class WtgHealthChecksFixture
	{
		[Test]
		public async Task QuartzWtgHealthChecks()
		{
			var stagingDbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			ConfigurationProvider.QuartzDefaultConnectionString = TestConnectionString.GetAdmin(stagingDbName);
			ConfigurationProvider.StagingConnectionString = TestConnectionString.GetAdmin(stagingDbName);
			ConfigurationProvider.QuartzProps["quartz.plugin.jobInitializer.fileNames"] = "~/TestConfiguration/TestJobs.xml";
			var uri = new Uri("http://localhost/quartz/wtg/health");

			using (var client = IntegrationTestHelper.WebAppFactory.CreateClient())
			using (var response = await client.GetAsync(uri))
			{
				response.EnsureSuccessStatusCode();
				var responseText = await response.Content.ReadAsStringAsync();
				Assert.AreEqual("INFO(Quartz): Service is OK", responseText);
			}

			ConfigurationProvider.QuartzDefaultConnectionString = TestConnectionString.GetAdmin(stagingDbName + "1");
			using (var client = IntegrationTestHelper.WebAppFactory.CreateClient())
			using (var response = await client.GetAsync(uri))
			{
				response.EnsureSuccessStatusCode();
				var responseText = await response.Content.ReadAsStringAsync();
				Assert.True(responseText.StartsWith("ERROR(Quartz): Service is unavailable"));
			}
		}
	}
}
