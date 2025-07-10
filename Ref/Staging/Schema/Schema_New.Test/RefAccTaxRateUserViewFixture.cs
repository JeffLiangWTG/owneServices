using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class RefAccTaxRateUserViewFixture
{
	[Test]
	public void Triggers()
	{
		var connectionString =
			TestConnectionString.GetAdmin(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging));
		// Insert
		using (var entities = new StagingRepository(connectionString))
		{
			var accTaxRateView = new RefAccTaxRateUserView
			{
				ZAT_PK = Guid.NewGuid(),
				ZAT_RN_NKCountry = "EU",
				ZAT_ReferenceRateType = "STD",
				ZAT_StartDate = new DateTime(1900, 01, 01),
				ZAT_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZAT_RateNumerator = 18,
				ZAT_RateDenominator = 1,
				ZAT_IsPublished = true
			};
			entities.Add(accTaxRateView);
			entities.SaveChanges();

			var accTaxRateSample = entities.Get<RefAccTaxRate>().FirstOrDefault();
			Assert.AreEqual("EU", accTaxRateSample.ZAT_RN_NKCountry);
			Assert.AreEqual("STD", accTaxRateSample.ZAT_ReferenceRateType);
			Assert.AreEqual(18, accTaxRateSample.ZAT_RateNumerator);
			Assert.AreEqual(1, accTaxRateSample.ZAT_RateDenominator);
			// Update
			accTaxRateView.ZAT_RN_NKCountry = "TG";
			accTaxRateView.ZAT_ReferenceRateType = "LOW";
			accTaxRateView.ZAT_RateNumerator = 21;
			accTaxRateView.ZAT_RateDenominator = 10;
			accTaxRateView.ZAT_IsPublished = false;
			entities.Update(accTaxRateView);
			entities.SaveChanges();
		}

		using (var entities = new StagingRepository(connectionString))
		{
			var accTaxRateSample = entities.Get<RefAccTaxRate>().FirstOrDefault();
			Assert.AreEqual("TG", accTaxRateSample.ZAT_RN_NKCountry);
			Assert.AreEqual("LOW", accTaxRateSample.ZAT_ReferenceRateType);
			Assert.AreEqual(21, accTaxRateSample.ZAT_RateNumerator);
			Assert.AreEqual(10, accTaxRateSample.ZAT_RateDenominator);
		}

		// Delete
		using (var entities = new StagingRepository(connectionString))
		{
			var accTaxRateViewSample = entities.Get<RefAccTaxRateUserView>().FirstOrDefault();
			entities.Remove(accTaxRateViewSample);
			entities.SaveChanges();
			Assert.Null(entities.Get<RefAccTaxRate>().FirstOrDefault());
		}
	}
}
