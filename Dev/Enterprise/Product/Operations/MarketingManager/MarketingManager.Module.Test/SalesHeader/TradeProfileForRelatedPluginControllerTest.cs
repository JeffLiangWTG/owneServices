using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(TradeProfileForRelatedPluginController))]
	public class TradeProfileForRelatedPluginControllerTest : ZControllerBasherTest
	{
		#region ID

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TradeProfileForRelatedPlugin;
		}

		#endregion

		#region PlugIn

		public void TestPluginTabPageCaption()
		{
			var controller = new TradeProfileForRelatedPluginController();
			AssertEquals("Value Analysis", controller.PluginTabPageCaption.Caption);
		}

		#endregion

		#region Security

		public void TestCheckPointForEdit()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var controller = new TradeProfileForRelatedPluginController();
			AssertEquals(Env.Security.ClientIntelligenceModifyTradeProfile, controller.GetCheckPointForEdit(opportunity));
		}

		public void TestCheckPointForView()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var controller = new TradeProfileForRelatedPluginController();
			AssertEquals(Env.Security.ClientIntelligenceViewTradeProfile, controller.GetCheckPointForView(opportunity));
		}

		#endregion
	}
}
