using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductTradeDetailFieldLayoutDefinitionCollection))]
	sealed class SalesProductTradeDetailFieldLayoutDefinitionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SalesProductTradeDetailFieldLayoutDefinitionCollection>
	{
		protected override SalesProductTradeDetailFieldLayoutDefinitionCollection GetCollectionToTest()
		{
			return new SalesProductTradeDetailFieldLayoutDefinitionCollection(SalesProduct);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SalesProductTradeDetailFieldLayoutDefinition(SalesProduct);
		}

		OrgSalesProduct SalesProduct
		{
			get { return salesProduct ?? (salesProduct = Factory.New<OrgSalesProduct>()); }
		}
		OrgSalesProduct salesProduct;
	}
}
