using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	class RefAccTaxRateUserViewFixture
	{
		[Test]
		public void View()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var accTaxRateView = new RefAccTaxRateUserView
				{
					ZAT_PK = Guid.NewGuid(),
					ZAT_RN_NKCountry = "PY",
					ZAT_ReferenceRateType = "STD",
					ZAT_StartDate = new DateTime(1900, 01, 01),
					ZAT_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					ZAT_RateNumerator = 19,
					ZAT_RateDenominator = 1
				};
				context.RefAccTaxRateUserViews.Add(accTaxRateView);
				context.SaveChanges();
				Assert.That(context.DataSetChangeHistories.Where(x => x.DCH_ParentPK == accTaxRateView.ZAT_PK).ToArray(), Has.Length.EqualTo(1));

				var refAccTaxRatesView = context.RefAccTaxRateUserViews.FirstOrDefault();
				Assert.AreEqual("PY", refAccTaxRatesView.ZAT_RN_NKCountry);
				Assert.AreEqual("STD", refAccTaxRatesView.ZAT_ReferenceRateType);
				Assert.AreEqual(19, refAccTaxRatesView.ZAT_RateNumerator);
				Assert.AreEqual(1, refAccTaxRatesView.ZAT_RateDenominator);

				//Update
				accTaxRateView.ZAT_RateNumerator = 30;
				accTaxRateView.ZAT_RateDenominator = 3;
				context.SaveChanges();
			}
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var refAccTaxRatesView = context.RefAccTaxRateUserViews.FirstOrDefault();
				Assert.AreEqual("PY", refAccTaxRatesView.ZAT_RN_NKCountry);
				Assert.AreEqual("STD", refAccTaxRatesView.ZAT_ReferenceRateType);
				Assert.AreEqual(30, refAccTaxRatesView.ZAT_RateNumerator);
				Assert.AreEqual(3, refAccTaxRatesView.ZAT_RateDenominator);
			}

			// Delete
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var refAccTaxRatesView = context.RefAccTaxRateUserViews.FirstOrDefault();
				context.RefAccTaxRateUserViews.Remove(refAccTaxRatesView);
				Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Should not enable Delete in top-level tables");
			}
		}
	}
}
