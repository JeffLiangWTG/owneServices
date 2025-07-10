using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class IntegrationTestFixture
	{
		[Test]
		public void IntegrationTest()
		{
			var uri = new Uri("http://localhost/odata/RefAccTaxRateUpdate");
			var result = string.Empty;
			Assert.DoesNotThrowAsync(async () =>
			{
				var stagingRepo = SetupRepoMock();
				var refDbRepoCrypto = new Mock<IRefDbRepoCrypto>();
				using (var factory = IntegrationTestHelper.GetWebAppFactory(stagingRepo, refDbRepoCrypto.Object))
				using (var client = factory.CreateClient())
				using (var response = await client.GetAsync(uri))
				{
					response.EnsureSuccessStatusCode();
					result = await response.Content.ReadAsStringAsync();
				}
			});
			Assert.AreEqual(refAccTaxRateResponse, result);
		}

		IStagingRepository SetupRepoMock()
		{
			var countryPk = Guid.NewGuid();
			var accTaxRatePk = Guid.Parse("7EDAE0A0-06E1-43C9-A8D9-33635B66F038");
			var country = new RefCountry
			{
				RN_PK = countryPk,
				RN_Code = "AA",
				RN_Desc = "AA",
				RN_ValidationStatus = "NAV"
			};
			var accTaxRate = new RefAccTaxRate
			{
				ZAT_PK = accTaxRatePk,
				ZAT_RN_NKCountry = "AA",
				ZAT_ReferenceRateType = "STD"
			};

			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<RefCountry>()).Returns(new[] { country }.AsQueryable());
			repo.Setup(x => x.Get<RefAccTaxRate>()).Returns(new[] { accTaxRate }.AsQueryable());
			return repo.Object;
		}

		readonly string refAccTaxRateResponse = "{\"@odata.context\":\"http://localhost/odata/$metadata#RefAccTaxRateUpdate\",\"value\":[{\"ZAT_PK\":\"7edae0a0-06e1-43c9-a8d9-33635b66f038\",\"ZAT_RN_NKCountry\":\"AA\",\"ZAT_ReferenceRateType\":\"STD\",\"ZAT_StartDate\":\"0001-01-01\",\"ZAT_EndDate\":\"0001-01-01\",\"ZAT_RateNumerator\":0,\"ZAT_RateDenominator\":0}]}";
	}
}
