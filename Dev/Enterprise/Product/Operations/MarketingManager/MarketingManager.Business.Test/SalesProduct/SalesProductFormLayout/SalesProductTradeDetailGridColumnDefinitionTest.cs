using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductTradeDetailGridColumnDefinition))]
	sealed class SalesProductTradeDetailGridColumnDefinitionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			return new SalesProductTradeDetailGridColumnDefinition(salesProduct);
		}
	}
}
