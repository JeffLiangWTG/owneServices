using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodUnlocoToUnlocoGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var period1 = NewTradePeriodForUnlocos(org.PK, "AUSYD", "AUSYD");
			var period2 = NewTradePeriodForUnlocos(org.PK, "AUSYD", "USNYC");
			var period3 = NewTradePeriodForUnlocos(org.PK, "AUSYD", "AUSYD");
			var period4 = NewTradePeriodForUnlocos(org.PK, "USNYC", "USNYC");
			var period5 = NewTradePeriodForUnlocos(org.PK, "AUSYD", null);
			var period6 = NewTradePeriodForUnlocos(org.PK, null, "AUSYD");

			Factory.Save();

			var grouper = new TradePeriodUnlocoToUnlocoGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradeDetails = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5, period6 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradeDetails);
			var groupings = grouper.GetGroupings(tradeDetails);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AUSYD -> AUSYD",
					"AUSYD -> USNYC",
					"USNYC -> USNYC",
					"AUSYD -> (Unknown)",
					"(Unknown) -> AUSYD",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period3 },
				groupings.Single(x => x.GroupKey == "AUSYD -> AUSYD").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period2 },
				groupings.Single(x => x.GroupKey == "AUSYD -> USNYC").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "USNYC -> USNYC").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5 },
				groupings.Single(x => x.GroupKey == "AUSYD -> (Unknown)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "(Unknown) -> AUSYD").GroupedTradePeriods);

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
