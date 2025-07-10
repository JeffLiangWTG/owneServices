using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductFieldLayoutDefinition))]
	sealed class SalesProductFieldLayoutDefinitionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			return new SalesProductTradeDetailFieldLayoutDefinition(salesProduct);
		}
	}
}
