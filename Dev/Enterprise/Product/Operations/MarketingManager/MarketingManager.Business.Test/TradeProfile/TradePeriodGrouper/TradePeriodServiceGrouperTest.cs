using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodServiceGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var period1 = NewTradePeriodForService("AAA");
			var period2 = NewTradePeriodForService("AAA");
			var period3 = NewTradePeriodForService("AAA");
			var period4 = NewTradePeriodForService("BBB");
			var period5 = NewTradePeriodForService("BBB");
			var period6 = NewTradePeriodForService("");

			var grouper = new TradePeriodServiceGrouper();
			var groupings = grouper.GetGroupings(new[] { period1, period2, period3, period4, period5, period6 });

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AAA",
					"BBB",
					"",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period2, period3 },
				groupings.Single(x => x.GroupKey == "AAA").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4, period5 },
				groupings.Single(x => x.GroupKey == "BBB").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "").GroupedTradePeriods);
		}

		OrgTradePeriod NewTradePeriodForService(ZString service)
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_Service = service;
			var detail = sales.TradeDetails.AddNew();
			return detail.TradedPeriods.AddNew();
		}
	}
}
