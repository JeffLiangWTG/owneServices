using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	class GuidGeneratorControllerFixture
	{
		[Test]
		public async Task GetNewGuid()
		{
			var uri = new Uri("http://localhost/api/GuidGenerator/Get");
			using (var factory = IntegrationTestHelper.GetWebAppFactory(null, null))
			using (var client = factory.CreateClient())
			using (var response = await client.GetAsync(uri))
			{
				response.EnsureSuccessStatusCode();
				var result = await response.Content.ReadAsStringAsync();
				Assert.True(!string.IsNullOrEmpty(result));
				var guid = Guid.Parse(result.Trim('"'));
				Assert.True(guid != Guid.Empty);
			}
		}
	}
}
