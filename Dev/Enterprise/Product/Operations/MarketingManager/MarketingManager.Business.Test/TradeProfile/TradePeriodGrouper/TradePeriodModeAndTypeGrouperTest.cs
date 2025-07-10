using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodModeAndTypeGrouperTest : TradePeriodGrouperTestCase
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

			var grouper = new TradePeriodModeAndTypeGrouper();
			var groupings = grouper.GetGroupings(new[] { period1, period2, period3, period4, period5, period6 });

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"Mode:AIR  Type:LSE",
					"Mode:AIR  Type:ULD",
					"Mode:SEA  Type:FCL",
					"Mode:AIR  Type:",
					"Mode:  Type:",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period3 },
				groupings.Single(x => x.GroupKey == "Mode:AIR  Type:LSE").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period2 },
				groupings.Single(x => x.GroupKey == "Mode:AIR  Type:ULD").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "Mode:SEA  Type:FCL").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5 },
				groupings.Single(x => x.GroupKey == "Mode:AIR  Type:").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "Mode:  Type:").GroupedTradePeriods);
		}
	}
}
