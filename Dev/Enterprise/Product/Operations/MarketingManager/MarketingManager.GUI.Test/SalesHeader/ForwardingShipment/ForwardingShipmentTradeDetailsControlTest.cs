using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ForwardingShipmentTradeDetailsControl))]
	class ForwardingShipmentTradeDetailsControlTest : TradeDetailsControlBaseTest
	{
		protected override TradeDetailsControl GetNewControlForTest()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			return new ForwardingShipmentTradeDetailsControl(product);
		}
	}
}
