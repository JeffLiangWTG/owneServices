using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class RateEntryFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForValidate()
		{
			var clientRate = new TestHelper(Factory).NewFullyPopulatedClientRate();
			var clientRateEntry = clientRate.AIRRateEntriesForBinding[0];
			Factory.Save();

			using (RowFactory.SetCachedTables())
			{
				var factory = new BusinessObjectFactory();
				var rateEntry = factory.ImportFromAnotherFactory(clientRateEntry) as RateEntry;

				var expectedHits = new Dictionary<string, int> { };

				AssertNotNull("Pre-condition", rateEntry);
				AssertDbHits(expectedHits, factory);

				rateEntry.Validation.ValidateAll();

				expectedHits = new Dictionary<string, int>
				{
					{ RatingHeaderSchema.Constants.TableName, 1 },
					{ RefCommodityCodeSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 3 },
				};

				AssertDbHits(expectedHits, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
			}
		}

		public void TestFetchForValidate_WhenLocationsAreNullOrShort_ShouldNotCrash()
		{
			var org = Factory.New<OrgHeader>();
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = org.PK;

			var rateEntry = clientRate.AddRateEntry("AIR", "LSE", "A", "B");

			AssertNoExceptionThrown(() => rateEntry.FetchStrategy.FetchForValidate());
		}
	}
}
