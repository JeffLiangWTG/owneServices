using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OpportunitySalesValueAnalysisCollection))]
	sealed class OpportunitySalesValueAnalysisCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OpportunitySalesValueAnalysisCollection>
	{
		public void TestRefresh()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var valueItem1 = opp.ValueItems.AddNew();
			var valueItem2 = opp.ValueItems.AddNew();

			var productA = Factory.NewWithValidTestData<OrgSalesProduct>();
			var productB = Factory.NewWithValidTestData<OrgSalesProduct>();
			var salesHeaderCollection = (SalesHeaderCollection)opp.ActualAndProspectiveSalesHeaderCollection;
			var salesHeaderA = salesHeaderCollection.AddNew(productA);
			var prospectSales = salesHeaderA.EntitySalesCollectionProductView.AddNew();
			var salesHeaderB = salesHeaderCollection.AddNew(productB);
			var actualSales = salesHeaderB.TradedSalesCollectionProductView.AddNew();
			actualSales.OW_IsTraded = true;

			Factory.Save();

			var collection = new OpportunitySalesValueAnalysisCollection(opp);
			collection.Refresh(true);
			AssertContainsExactElementsInAnyOrder(
				new BusinessObject[]
				{
					valueItem1,
					valueItem2,
					productA
				},
				collection.Cast<OpportunitySalesValueAnalysis>().Select(x => (BusinessObject)x.SalesHeader?.SalesProduct ?? x.ValueItem));
		}

		public void TestAllowNew()
		{
			var opp = Factory.New<OrgOpportunity>();
			var collection = new OpportunitySalesValueAnalysisCollection(opp);
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var opp = Factory.New<OrgOpportunity>();
			var collection = new OpportunitySalesValueAnalysisCollection(opp);
			AssertEquals(false, collection.AllowRemove);
		}

		protected override OpportunitySalesValueAnalysisCollection GetCollectionToTest()
		{
			var opp = Factory.New<OrgOpportunity>();
			return new OpportunitySalesValueAnalysisCollection(opp);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, salesProduct);
			return new OpportunitySalesValueAnalysis(salesHeader);
		}
	}
}
