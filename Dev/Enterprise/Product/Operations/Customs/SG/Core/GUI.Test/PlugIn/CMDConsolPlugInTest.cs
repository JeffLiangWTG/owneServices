using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class CMDConsolPlugInTest : TestCaseWithFactory
	{
		public void TestBusinessObject()
		{
			AssertEquals(typeof(CMDConsolWrapper), plugIn.BusinessEntity.GetType());
			AssertEquals(consol, ((CMDConsolWrapper)plugIn.BusinessEntity).Consol);
		}

		public void TestTransportMode()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModes.Air, plugIn.TransportMode);
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(Core.Constants.TransportModes.Road, plugIn.TransportMode);
		}

		public void TestHasUserControl()
		{
			AssertNull(plugIn.UserControl);
		}

		public void TestHookUnhookEvents()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "SGSIN";
			Assert("Pre-condition - Export Consol", plugIn.Enabled);
			plugIn.UnhookEvents();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("Event is unhooked, should not be updated", plugIn.Enabled);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Assert("Event is unhooked, should not be updated", plugIn.Enabled);
			plugIn.HookEvents();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			Assert("Plugin should not apply - does not relate to SG", !plugIn.Enabled);
			consol.JK_RL_NKDischargePort = "SGSIN";
			Assert("Import Consol - Plugin should  apply", plugIn.Enabled);
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("Event now hooked, should be updated", !plugIn.Enabled);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			plugIn = new CMDConsolPlugIn(consol);
		}

		protected override void TearDown()
		{
			plugIn.Dispose();
			base.TearDown();
		}
		ForwardingConsol consol;
		CMDConsolPlugIn plugIn;
	}
}
