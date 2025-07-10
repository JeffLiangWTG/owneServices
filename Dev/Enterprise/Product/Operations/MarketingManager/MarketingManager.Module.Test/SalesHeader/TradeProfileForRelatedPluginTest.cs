using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Module.Testing
{
	class TradeProfileForRelatedPluginTest : TestCaseWithFactory
	{
		#region Name

		public void TestName()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			using (var plugin = new TradeProfileForRelatedPlugin(opportunity))
			{
				AssertEquals("Value Analysis", plugin.Name);
			}
		}

		#endregion

		#region User Control

		public void TestUserControl()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			using (var plugin = new TradeProfileForRelatedPlugin(opportunity))
			using (var control = plugin.UserControl)
			{
				AssertType(typeof(TradeProfileForRelatedUserControl), control);
			}
		}

		#endregion
	}
}
