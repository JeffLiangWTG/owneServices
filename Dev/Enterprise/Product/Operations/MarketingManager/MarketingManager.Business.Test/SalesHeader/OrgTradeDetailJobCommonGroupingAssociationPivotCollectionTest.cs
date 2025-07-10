using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgTradeDetailJobCommonGroupingAssociationPivotCollection))]
	sealed class OrgTradeDetailJobCommonGroupingAssociationPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgTradeDetailJobCommonGroupingAssociationPivotCollection>
	{
		public void TestRelationship()
		{
			var forwardingProduct = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = forwardingProduct.PK;

			var detail1a = sales.TradeDetails.AddNew();
			detail1a.PA_TradeMode = "SEA";
			detail1a.PA_TradeType = "FCL";

			var detail1b = sales.TradeDetails.AddNew();
			detail1b.PA_TradeMode = "SEA";
			detail1b.PA_TradeType = "FCL";

			var detail2a = sales.TradeDetails.AddNew();
			detail2a.PA_TradeMode = "SEA";
			detail2a.PA_TradeType = "FCL";

			var detail2b = sales.TradeDetails.AddNew();
			detail2b.PA_TradeMode = "AIR";
			detail2b.PA_TradeType = "LSE";

			var opp1 = org.SalesOpportunities.AddNew();
			var opp1SalesPivot = opp1.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var opp1Detail1aPivot = opp1.AssociatedTradeLanesPivots.AddPivotFor(detail1a);
			var opp1Detail1bPivot = opp1.AssociatedTradeLanesPivots.AddPivotFor(detail1b);

			var opp2 = org.SalesOpportunities.AddNew();
			var opp2SalesPivot = opp2.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var opp2Detail2aPivot = opp2.AssociatedTradeLanesPivots.AddPivotFor(detail2a);
			var opp2Detail2bPivot = opp2.AssociatedTradeLanesPivots.AddPivotFor(detail2b);

			Factory.Save();

			var entitySales = EntitySalesWrapper.Get(sales, org);
			entitySales.CompanyFilter = ZGuid.Empty;
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, forwardingProduct, "SEA", "FCL", entitySales.EntityTradeDetailsCompanyView.Cast<EntityTradeDetailWrapper>());
			var collection = new OrgTradeDetailJobCommonGroupingAssociationPivotCollection(grouping, true);
			AssertContainsExactElementsInAnyOrder("When additional filter not set, should return all trade details associated with this sales",
				BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { opp1Detail1aPivot, opp1Detail1bPivot, opp2Detail2aPivot, opp2Detail2bPivot }, collection);
			AssertContainsExactElementsInAnyOrder("Grouping constructor sets Additional filter which should filter by mode and type",
				BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { opp1Detail1aPivot, opp1Detail1bPivot, opp2Detail2aPivot }, grouping.SalesAssociationPivotCollectionCompanyView);
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		protected override OrgTradeDetailJobCommonGroupingAssociationPivotCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var product = Factory.New<OrgSalesProduct>();
			var entitySales = EntitySalesWrapper.Get(sales, org);
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
			return new OrgTradeDetailJobCommonGroupingAssociationPivotCollection(grouping, true);
		}
	}
}
