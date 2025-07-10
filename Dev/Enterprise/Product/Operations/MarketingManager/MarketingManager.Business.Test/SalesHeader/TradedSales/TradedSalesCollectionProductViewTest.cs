using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(TradedSalesCollectionProductView))]
	sealed class TradedSalesCollectionProductViewTest : BusinessObjectCollectionViewTestCase<TradedSalesCollectionProductView>
	{
		#region Relationship

		public void TestRelationship()
		{
			var salesProduct1 = Factory.New<OrgSalesProduct>();
			var salesProduct2 = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesCollection = org.SalesCollection;
			var sales1A = salesCollection.AddNew();
			sales1A.OW_MP_Product = salesProduct1.PK;
			sales1A.OW_IsTraded = true;
			var sales1B = salesCollection.AddNew();
			sales1B.OW_MP_Product = salesProduct1.PK;
			sales1B.OW_IsTraded = true;

			var sales2A = salesCollection.AddNew();
			sales2A.OW_MP_Product = salesProduct2.PK;
			sales2A.OW_IsTraded = true;
			var sales2B = salesCollection.AddNew();
			sales2B.OW_MP_Product = salesProduct2.PK;
			sales2B.OW_IsTraded = true;

			var sales0A = salesCollection.AddNew();
			sales0A.OW_MP_Product = ZGuid.Empty;
			sales0A.OW_IsTraded = true;
			var sales0B = salesCollection.AddNew();
			sales0B.OW_MP_Product = ZGuid.Empty;
			sales0B.OW_IsTraded = true;

			var tradedCollection = new TradedSalesCollection(org);
			var collection = new TradedSalesCollectionProductView(tradedCollection, null);
			AssertContainsExactElementsInAnyOrder(new[] { sales0A, sales0B }, collection);

			collection = new TradedSalesCollectionProductView(tradedCollection, salesProduct1);
			AssertContainsExactElementsInAnyOrder(new[] { sales1A, sales1B }, collection);

			collection = new TradedSalesCollectionProductView(tradedCollection, salesProduct2);
			AssertContainsExactElementsInAnyOrder(new[] { sales2A, sales2B }, collection);
		}

		#endregion

		#region Overrides

		protected override TradedSalesCollectionProductView GetCollectionToTest()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var tradedCollection = new TradedSalesCollection(org);
			return new TradedSalesCollectionProductView(tradedCollection, salesProduct);
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
