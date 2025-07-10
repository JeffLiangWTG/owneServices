using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OrgSalesProspectCompanyBulkPopulaterTest : TestCaseWithFactory
	{
		public void TestExecute()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			opp1.P8_GC = company1.PK;
			var opp2 = org.SalesOpportunities.AddNew();
			opp2.P8_GC = company2.PK;

			var forwardingProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales1 = Factory.New<OrgSales>();
			sales1.OW_MP_Product = forwardingProduct.PK;
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = org.PK;

			var detail1a = sales1.TradeDetails.AddNew();
			var detail1b = sales1.TradeDetails.AddNew();

			var sales2 = Factory.New<OrgSales>();
			sales2.OW_MP_Product = forwardingProduct.PK;
			sales2.OW_IsTraded = false;
			sales2.OW_OH_Primary = org.PK;

			var detail2 = sales2.TradeDetails.AddNew();

			sales1.SalesAssociationPivotCollectionGlobal.AddNew(org);

			opp1.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			opp1.AssociatedTradeLanesPivots.AddPivotFor(detail1a);

			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales1);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(sales2);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(detail1b);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(detail2);

			Factory.Save();

			var entitySales1 = EntitySalesWrapper.Get(sales1, org);
			var entitySales2 = EntitySalesWrapper.Get(sales2, org);
			var entityDetail1a = entitySales1.EntityTradeDetails.Single(x => x.PK == detail1a.PK);
			var entityDetail1b = entitySales1.EntityTradeDetails.Single(x => x.PK == detail1b.PK);
			var entityDetail2 = entitySales2.EntityTradeDetails.Single(x => x.PK == detail2.PK);

			var populater = new OrgSalesProspectCompanyBulkPopulater(Factory, org.PK);
			populater.Execute(new EntitySalesWrapper[] { entitySales1, entitySales2 });

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { ZGuid.Empty, company1.PK, company2.PK }, entitySales1.ProspectCompanyPks);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { company2.PK }, entitySales2.ProspectCompanyPks);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { company1.PK }, entityDetail1a.ProspectCompanyPks);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { company2.PK }, entityDetail1b.ProspectCompanyPks);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { company2.PK }, entityDetail2.ProspectCompanyPks);
		}
	}
}
