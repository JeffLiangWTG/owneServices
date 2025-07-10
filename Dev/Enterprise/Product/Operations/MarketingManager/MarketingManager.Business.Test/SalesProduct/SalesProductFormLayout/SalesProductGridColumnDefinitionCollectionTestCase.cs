using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	abstract class SalesProductGridColumnDefinitionCollectionTestCase<T, U> : NonPersistentBusinessObjectCollectionTestCase<T> where T : SalesProductGridColumnDefinitionCollection<U> where U : SalesProductGridColumnDefinition
	{
		public void TestDefaultValues()
		{
			var collection = GetCollectionToTest();
			AssertEquals(1, collection.AddNew().Order);
			AssertEquals(2, collection.AddNew().Order);
			AssertEquals(3, collection.AddNew().Order);
		}

		protected OrgSalesProduct SalesProduct
		{
			get { return salesProduct ?? (salesProduct = Factory.New<OrgSalesProduct>()); }
		}
		OrgSalesProduct salesProduct;
	}
}
