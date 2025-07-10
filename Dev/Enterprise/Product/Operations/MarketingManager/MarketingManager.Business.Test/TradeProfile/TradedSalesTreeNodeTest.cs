using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradedSalesTreeNodeTest : TestCaseWithFactory
	{
		public void TestChildren_Warehouse_DoNotIncludeJobTotalsWhenGroupedBySupplierPart()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, warehouseProduct);
			var tradedSalesAnalysis = new TradedSalesAnalysis(salesHeader);

			var part1 = Factory.New<OrgSupplierPart>();
			var tradeDetail1A = Factory.New<OrgTradeDetail>();
			tradeDetail1A.PA_OP = part1.PK;
			var period1A = tradeDetail1A.TradedPeriods.AddNew();

			var tradeDetail1B = Factory.New<OrgTradeDetail>();
			tradeDetail1B.PA_OP = part1.PK;
			var period1B = tradeDetail1B.TradedPeriods.AddNew();

			var tradeDetailJobTotals = Factory.New<OrgTradeDetail>();
			tradeDetailJobTotals.PA_OP = ZGuid.Empty;
			var periodJob = tradeDetailJobTotals.TradedPeriods.AddNew();

			var part1Grouping = new TradePeriodGrouping(tradedSalesAnalysis, new[] { period1A, period1B }, typeof(TradePeriodSupplierPartGrouper));
			var jobTotalGrouping = new TradePeriodGrouping(tradedSalesAnalysis, new[] { periodJob }, typeof(TradePeriodSupplierPartGrouper));

			var grouping = new TradePeriodGrouping(tradedSalesAnalysis, "", new[] { part1Grouping, jobTotalGrouping }, null);
			var model = new TradedSalesTreeModel(tradedSalesAnalysis);
			var node = new TradedSalesTreeNode(model, grouping);
			AssertContainsExactElementsInAnyOrder("Should not include Job total child groupings when grouped by supplier part", new[] { part1Grouping }, node.ChildNodes.Select(x => x.BizObj));
		}
	}
}
