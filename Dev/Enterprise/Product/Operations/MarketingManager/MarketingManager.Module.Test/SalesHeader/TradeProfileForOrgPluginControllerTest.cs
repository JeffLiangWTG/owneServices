using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(TradeProfileForOrgPluginController))]
	public class TradeProfileForOrgPluginControllerTest : ZControllerBasherTest
	{
		#region ID

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TradeProfileForOrgPlugin;
		}

		#endregion

		#region PlugIn

		public void TestPluginTabPageCaption()
		{
			var controller = new TradeProfileForOrgPluginController();
			AssertEquals("Value Analysis", controller.PluginTabPageCaption.Caption);
		}

		#endregion

		#region Security

		public void TestCheckPointForEdit()
		{
			var org = Factory.New<OrgHeader>();
			var controller = new TradeProfileForOrgPluginController();
			AssertEquals(Env.Security.ClientIntelligenceModifyTradeProfile, controller.GetCheckPointForEdit(org));
		}

		public void TestCheckPointForView()
		{
			var org = Factory.New<OrgHeader>();
			var controller = new TradeProfileForOrgPluginController();
			AssertEquals(Env.Security.ClientIntelligenceViewTradeProfile, controller.GetCheckPointForView(org));
		}

		#endregion
	}
}
