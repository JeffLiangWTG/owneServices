using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GenericTradeDetailsControl))]
	class GenericTradeDetailsControlTest : TradeDetailsControlBaseTest
	{
		protected override TradeDetailsControl GetNewControlForTest()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			return new GenericTradeDetailsControl(salesProduct);
		}
	}
}
