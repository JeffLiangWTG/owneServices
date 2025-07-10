using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	[CreateWebPortalService]
	public class CreateWebPortalServiceAttributeFixture
	{
		[Test]
		public async Task ShouldStartWebPortalServiceAndServeStaticFiles()
		{
			var uri = new Uri(CreateWebPortalServiceAttribute.Urls);
			using (var client = new HttpClient())
			{
				var response = await client.GetAsync(uri);
				response.EnsureSuccessStatusCode();
				var responseBody = await response.Content.ReadAsStringAsync();
				Assert.That(responseBody, Contains.Substring("<title>Reference Service"));
			}
		}
	}
}
