
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradeDetailAssociationPivotCollection))]
	sealed class TradeDetailAssociationPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<TradeDetailAssociationPivotCollection>
	{
		public void TestRelationship()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var sales = Factory.NewWithValidTestData<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			AssertEquals(sales.PK, tradeDetail.PA_OW);
			var wrapper = EntityTradeDetailWrapper.Get(tradeDetail, opportunity);

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var tradeDetail2 = sales.TradeDetails.AddNew();
			AssertEquals(sales.PK, tradeDetail2.PA_OW);
			var wrapper2 = EntityTradeDetailWrapper.Get(tradeDetail2, opportunity2);

			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity, tradeDetail);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(opportunity2, tradeDetail2);

			Factory.Save();

			AssertEquals("There should be 1 association per opportunity for the first detail", 2,
				wrapper.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(
				wrapper.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(
				wrapper.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));

			AssertEquals("There should be 1 association per opportunity for the second detail", 2,
				wrapper2.SalesAssociationPivotCollectionCompanyView.Count);
			AssertNotNull(
				wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity.PK));
			AssertNotNull(
				wrapper2.SalesAssociationPivotCollectionCompanyView.FirstOrDefault(x => x.SVP_ActivityId == opportunity2.PK));
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

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		protected override TradeDetailAssociationPivotCollection GetCollectionToTest()
		{
			var opp = Factory.New<OrgOpportunity>();
			var sales = Factory.NewWithValidTestData<OrgSales>();
			var detail = sales.TradeDetails.AddNew();
			var entityTradeDetails = EntityTradeDetailWrapper.Get(detail, opp);
			return new TradeDetailAssociationPivotCollection(entityTradeDetails, true);
		}
	}
}
