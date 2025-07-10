using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GenericTradeLanesControl))]
	class GenericTradeLanesControlTest : TradeLanesControlBaseTest
	{
		protected override TradeLanesControl GetNewControlForTest()
		{
			var product = Factory.New<OrgSalesProduct>();
			return new GenericTradeLanesControl(product);
		}
	}
}
