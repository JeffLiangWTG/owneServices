using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(JobCommonTradeLanesControl))]
	class JobCommonTradeLanesControlTest : TradeLanesControlBaseTest
	{
		protected override TradeLanesControl GetNewControlForTest()
		{
			var product = Factory.New<OrgSalesProduct>();
			return new JobCommonTradeLanesControl(product);
		}
	}
}
