using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductTradeLaneGridColumnDefinitionCollection))]
	sealed class SalesProductTradeLaneGridColumnDefinitionCollectionTest : SalesProductGridColumnDefinitionCollectionTestCase<SalesProductTradeLaneGridColumnDefinitionCollection, SalesProductTradeLaneGridColumnDefinition>
	{
		protected override SalesProductTradeLaneGridColumnDefinitionCollection GetCollectionToTest()
		{
			return new SalesProductTradeLaneGridColumnDefinitionCollection(SalesProduct);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SalesProductTradeLaneGridColumnDefinition(SalesProduct);
		}
	}
}
