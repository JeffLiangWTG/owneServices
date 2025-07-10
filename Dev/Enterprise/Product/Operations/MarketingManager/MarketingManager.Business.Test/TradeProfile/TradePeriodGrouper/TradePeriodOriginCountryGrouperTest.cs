using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodOriginCountryGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var period1 = NewTradePeriodForCountries(org.PK, "AU", "AU");
			var period2 = NewTradePeriodForCountries(org.PK, "AU", "US");
			var period3 = NewTradePeriodForCountries(org.PK, "AU", "AU");
			var period4 = NewTradePeriodForCountries(org.PK, "US", "US");
			var period5 = NewTradePeriodForCountries(org.PK, "AU", null);
			var period6 = NewTradePeriodForCountries(org.PK, null, "AU");

			Factory.Save();

			var grouper = new TradePeriodOriginCountryGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradePeriods = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5, period6 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradePeriods);
			var groupings = grouper.GetGroupings(tradePeriods);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"Origin: AU (Australia)",
					"Origin: US (United States)",
					"Origin: (Unknown)",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period2, period3, period5 },
				groupings.Single(x => x.GroupKey == "Origin: AU (Australia)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "Origin: US (United States)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "Origin: (Unknown)").GroupedTradePeriods);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ ViewLocationSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}
	}
}
