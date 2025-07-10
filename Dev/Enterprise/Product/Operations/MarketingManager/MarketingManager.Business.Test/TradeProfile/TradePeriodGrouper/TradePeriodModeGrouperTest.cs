using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodModeGrouperTest : TradePeriodGrouperTestCase
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

			var grouper = new TradePeriodModeGrouper();
			var groupings = grouper.GetGroupings(new[] { period1, period2, period3, period4, period5, period6 });

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AIR",
					"SEA",
					"",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period2, period3, period5 },
				groupings.Single(x => x.GroupKey == "AIR").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "SEA").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "").GroupedTradePeriods);
		}
	}
}
