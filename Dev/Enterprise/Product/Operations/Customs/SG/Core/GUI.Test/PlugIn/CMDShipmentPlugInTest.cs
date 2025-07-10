using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Customs.SG.V4.GUI.CMDMessaging;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class CMDShipmentPlugInTest : TestCaseWithFactory
	{
		public void TestBusinessObject()
		{
			AssertEquals(typeof(CMDShipmentWrapper), plugIn.BusinessEntity.GetType());
			AssertEquals(shipment, ((CMDShipmentWrapper)plugIn.BusinessEntity).Shipment);
		}

		public void TestTransportMode()
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModes.Air, plugIn.TransportMode);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals(Core.Constants.TransportModes.Road, plugIn.TransportMode);
		}

		public void TestHasUserControl()
		{
			AssertType<CMDShipmentUserControl>(plugIn.UserControl);
		}

		public void TestHookUnhookEvents()
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Assert("Pre-condition", plugIn.Enabled);
			plugIn.UnhookEvents();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("Event is unhooked, should not be updated", plugIn.Enabled);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Assert("Event is unhooked, should not be updated", plugIn.Enabled);
			plugIn.HookEvents();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("Event now hooked, should be updated", !plugIn.Enabled);
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
			plugIn = new CMDShipmentPlugIn(shipment);
		}

		protected override void TearDown()
		{
			plugIn.Dispose();
			base.TearDown();
		}
		ForwardingShipment shipment;
		CMDShipmentPlugIn plugIn;
	}
}
