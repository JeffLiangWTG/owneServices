using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class WtgStatusFixture
	{
		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task HealthCheck()
		{
			DbConnectionStringManager.SetConfigFileForTest("ConnectionStrings.SafeUpdateService.Test.config.json");
			var uri = new Uri("http://localhost/wtg/status");
			using (var factory = IntegrationTestHelper.WebAppFactory)
			using (var client = factory.CreateClient())
			using (var response = await client.GetAsync(uri))
			{
				response.EnsureSuccessStatusCode();
				var responseText = await response.Content.ReadAsStringAsync();
				Assert.That("INFO(UpdateService): Service is OK", Is.EqualTo(responseText), $"responseText is {responseText}, please check it.");
			}
		}
	}
}
