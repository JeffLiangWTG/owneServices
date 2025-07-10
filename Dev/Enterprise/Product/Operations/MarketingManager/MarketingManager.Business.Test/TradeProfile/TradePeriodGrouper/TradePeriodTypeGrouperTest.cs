using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodTypeGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var period1 = NewTradePeriodForModeAndType(org.PK, "AIR", "LSE");
			var period2 = NewTradePeriodForModeAndType(org.PK, "AIR", "ULD");
			var period3 = NewTradePeriodForModeAndType(org.PK, "AIR", "LSE");
			var period4 = NewTradePeriodForModeAndType(org.PK, "SEA", "FCL");
			var period5 = NewTradePeriodForModeAndType(org.PK, "AIR", "");
			var period6 = NewTradePeriodForModeAndType(org.PK, "", "");

			var grouper = new TradePeriodTypeGrouper();
			var tradePeriods = new[] { period1, period2, period3, period4, period5, period6 };
			grouper.AddFetchHints(tradePeriods);
			var groupings = grouper.GetGroupings(tradePeriods);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"LSE",
					"ULD",
					"FCL",
					"",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period3 },
				groupings.Single(x => x.GroupKey == "LSE").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period2 },
				groupings.Single(x => x.GroupKey == "ULD").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "FCL").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5, period6 },
				groupings.Single(x => x.GroupKey == "").GroupedTradePeriods);
		}
	}
}
