using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class WtgStatusFixture
	{
		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task HealthCheck()
		{
			DbConnectionStringManager.SetConfigFileForTest("ConnectionStrings.DeliveryService.Test.config.json");
			var uri = new Uri("http://localhost/wtg/status");
			using (var client = WebApplicationFactoryHelper.WebAppFactory.CreateClient())
			using (var response = await client.GetAsync(uri))
			{
				response.EnsureSuccessStatusCode();
				var responseText = await response.Content.ReadAsStringAsync();
				Assert.That(responseText.StartsWith("INFO(DeliveryService): Service is OK", StringComparison.OrdinalIgnoreCase));
			}
		}
	}
}
