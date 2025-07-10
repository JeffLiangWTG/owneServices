using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(WarehouseTradeLanesControl))]
	class WarehouseTradeLanesControlTest : TradeLanesControlBaseTest
	{
		protected override TradeLanesControl GetNewControlForTest()
		{
			var product = Factory.New<OrgSalesProduct>();
			return new WarehouseTradeLanesControl(product);
		}
	}
}
