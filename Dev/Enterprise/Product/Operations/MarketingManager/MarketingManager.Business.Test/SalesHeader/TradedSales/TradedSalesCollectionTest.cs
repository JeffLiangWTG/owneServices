using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradedSalesCollection))]
	sealed class TradedSalesCollectionTest : BusinessObjectCollectionViewTestCase<TradedSalesCollection>
	{
		#region Relationship

		public void TestRelationship()
		{
			var org = Factory.New<OrgHeader>();
			var salesCollection = org.SalesCollection;
			var prospectSales1 = salesCollection.AddNew();
			prospectSales1.OW_IsTraded = false;
			var tradedSales1 = salesCollection.AddNew();
			tradedSales1.OW_IsTraded = true;
			var tradedSales2 = salesCollection.AddNew();
			tradedSales2.OW_IsTraded = true;

			var collection = new TradedSalesCollection(org);
			AssertContainsExactElementsInAnyOrder(new[] { tradedSales1, tradedSales2 }, collection);
		}

		#endregion

		#region Overrides

		protected override TradedSalesCollection GetCollectionToTest()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			return new TradedSalesCollection(org);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_IsTraded = true;
			return sales;
		}

		#endregion
	}
}
