using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[CreateDatabase("217710E7CC3F420B8F54B77198888E31", DbSchema.RefDbRepoSafe,
		"97D7E6A6D4AC443494CDAB78AFF3A398", DbSchema.RefDbRepoStaging)]
	[CreateStagingService("97D7E6A6D4AC443494CDAB78AFF3A398")]
	public class CreateStagingServiceAttributeFixture
	{
		[Test]
		public async Task ShouldStartStagingServiceAndServeOdataUrl()
		{
			const string baseUrl = CreateStagingServiceAttribute.Urls;
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
