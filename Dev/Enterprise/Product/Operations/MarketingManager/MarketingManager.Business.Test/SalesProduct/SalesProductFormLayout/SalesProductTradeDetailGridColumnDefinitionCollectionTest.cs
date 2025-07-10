using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductTradeDetailGridColumnDefinitionCollection))]
	sealed class SalesProductTradeDetailGridColumnDefinitionCollectionTest : SalesProductGridColumnDefinitionCollectionTestCase<SalesProductTradeDetailGridColumnDefinitionCollection, SalesProductTradeDetailGridColumnDefinition>
	{
		protected override SalesProductTradeDetailGridColumnDefinitionCollection GetCollectionToTest()
		{
			return new SalesProductTradeDetailGridColumnDefinitionCollection(SalesProduct);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SalesProductTradeDetailGridColumnDefinition(SalesProduct);
		}
	}
}
