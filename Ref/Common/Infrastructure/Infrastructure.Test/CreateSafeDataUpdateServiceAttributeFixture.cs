using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[CreateDatabase("5CF49B8488174D30BA7E156EB9D746AD", DbSchema.RefDbRepoSafe,
		"F2EF2D8BA61F4CA89F336369D207AAE8", DbSchema.RefDbRepoStaging)]
	[CreateSafeDataUpdateService("5CF49B8488174D30BA7E156EB9D746AD")]
	public class CreateSafeDataUpdateServiceAttributeFixture
	{
		[Test]
		public async Task ShouldStartSafeUpdateServiceAndServeOdataUrl()
		{
			const string baseUrl = CreateSafeDataUpdateServiceAttribute.Urls;
			var uri = new Uri($"{baseUrl}odata");
			using (var client = new HttpClient())
			{
				var response = await client.GetAsync(uri);
				response.EnsureSuccessStatusCode();
				var responseBody = await response.Content.ReadAsStringAsync();

				var odataContext = $"\"@odata.context\":\"{baseUrl}odata/$metadata\"";
				Assert.That(responseBody, Contains.Substring(odataContext));
			}
		}
	}
}
