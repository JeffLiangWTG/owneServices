using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.NewService.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	class EntityMatcherControllerFixture
	{
		[Test]
		public async Task GetCodes()
		{
			var uri = "http://localhost/api/EntityMatcher/GetCodes?entityClass=COUNTRY&language=EN";
			var matcher = new Mock<IEntityMatcher>();
			var names = new[] { "AU", "ZA" };
			uri += string.Concat(names.Select(x => $"&names={x}"));
			var matchingCodes = new[] { new MatchingCode { Score = 10, Result = "Result1" }, new MatchingCode { Score = 20, Result = "Result2" } };
			matcher.Setup(x => x.GetBestMatchingCodes(EntityClass.COUNTRY, "EN", true, true, names)).Returns(matchingCodes);
			using (var webAppFactory = IntegrationTestHelper.GetWebAppFactory(null, null).WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.AddScoped(sp => matcher.Object);
				});
			}))
			using (var client = webAppFactory.CreateClient())
			using (var response = await client.GetAsync(new Uri(uri)))
			{
				response.EnsureSuccessStatusCode();
				var result = await response.Content.ReadAsStringAsync();
				Assert.True(!string.IsNullOrEmpty(result));
				Assert.AreEqual("[\r\n  \"Result1\",\r\n  \"Result2\"\r\n]", result);
			}
		}

		[Test]
		public void InvalidEntityClassThrowsException()
		{
			var matcher = new Mock<IEntityMatcher>();
			var controller = new EntityMatcherController(matcher.Object);
			var exception = Assert.Throws<NotSupportedException>(() => controller.GetCodes("XXX", Array.Empty<string>()));
			Assert.AreEqual("Entity Class XXX is not supported.", exception.Message);
		}
	}
}
