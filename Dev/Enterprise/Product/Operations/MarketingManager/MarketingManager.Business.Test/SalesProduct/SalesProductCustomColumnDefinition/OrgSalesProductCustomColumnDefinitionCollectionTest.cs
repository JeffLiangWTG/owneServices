using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgSalesProductCustomColumnDefinitionCollection))]
	sealed class OrgSalesProductCustomColumnDefinitionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultValues()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();

			var detailCollection = new OrgSalesProductCustomColumnDefinitionCollection(salesProduct, SalesProductDefinitionGroup.Lane);
			var newElement = detailCollection.AddNew();
			AssertEquals(salesProduct.PK, newElement.XC_ParentID);
			AssertEquals(salesProduct.TablePrefix, newElement.XC_ParentTableCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			return new OrgSalesProductCustomColumnDefinitionCollection(salesProduct, SalesProductDefinitionGroup.Lane);
		}
	}
}
