using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodCountryToCountryGrouperTest : TradePeriodGrouperTestCase
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

			var grouper = new TradePeriodCountryToCountryGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradePeriods = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5, period6 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradePeriods);
			var groupings = grouper.GetGroupings(tradePeriods);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AU (Australia) -> AU (Australia)",
					"AU (Australia) -> US (United States)",
					"US (United States) -> US (United States)",
					"AU (Australia) -> (Unknown)",
					"(Unknown) -> AU (Australia)",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period3 },
				groupings.Single(x => x.GroupKey == "AU (Australia) -> AU (Australia)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period2 },
				groupings.Single(x => x.GroupKey == "AU (Australia) -> US (United States)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "US (United States) -> US (United States)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5 },
				groupings.Single(x => x.GroupKey == "AU (Australia) -> (Unknown)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "(Unknown) -> AU (Australia)").GroupedTradePeriods);

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
