using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodCompanyGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var period1 = NewTradePeriodForModeAndType(org.PK, "AIR", "IMP");
			var period2 = NewTradePeriodForModeAndType(org.PK, "SEA", "EXP");
			var period3 = NewTradePeriodForModeAndType(org.PK, "AIR", "EXP");

			var customsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);

			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";
			company1.GC_RN_NKCountryCode = "CN";

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "BBB";
			company2.GC_RN_NKCountryCode = "NZ";

			var sales1 = period1.TradeDetail.Sales;
			sales1.OW_MP_Product = customsProduct.PK;
			sales1.OW_GC = company1.PK;

			var sales2 = period2.TradeDetail.Sales;
			sales2.OW_MP_Product = customsProduct.PK;
			sales2.OW_GC = company2.PK;

			var sales3 = period3.TradeDetail.Sales;
			sales3.OW_MP_Product = customsProduct.PK;

			var grouper = new TradePeriodCompanyGrouper();
			var tradePeriods = new[] { period1, period2, period3 };
			grouper.AddFetchHints(tradePeriods);
			var groupings = grouper.GetGroupings(tradePeriods);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AAA[CN]",
					"BBB[NZ]",
					"(Unknown Company)"
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1 },
				groupings.Single(x => x.GroupKey == "AAA[CN]").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period2 },
				groupings.Single(x => x.GroupKey == "BBB[NZ]").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period3 },
				groupings.Single(x => x.GroupKey == "(Unknown Company)").GroupedTradePeriods);
		}
	}
}
