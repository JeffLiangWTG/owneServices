using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductGridColumnDefinition))]
	sealed class SalesProductGridColumnDefinitionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var columnDefinition = new SalesProductTradeLaneGridColumnDefinition(salesProduct);
			AssertEquals(true, columnDefinition.IsVisible);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			return new SalesProductTradeLaneGridColumnDefinition(salesProduct);
		}
	}
}
