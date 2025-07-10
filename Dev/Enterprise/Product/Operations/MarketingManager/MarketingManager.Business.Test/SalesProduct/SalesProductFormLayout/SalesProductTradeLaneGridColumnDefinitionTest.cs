using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductTradeLaneGridColumnDefinition))]
	sealed class SalesProductTradeLaneGridColumnDefinitionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			return new SalesProductTradeLaneGridColumnDefinition(salesProduct);
		}
	}
}
