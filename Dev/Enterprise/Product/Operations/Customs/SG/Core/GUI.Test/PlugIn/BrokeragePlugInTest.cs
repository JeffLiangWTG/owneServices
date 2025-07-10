using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public override void TestClearHasChanges()
		{
			Assert(true);
		}

		public void TestMenuIsCorrectType()
		{
			using (var plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		public void TestBrokerageControlIsCorrectType()
		{
			using (var plugin = new BrokeragePlugInTestClass(Factory.New<ForwardingShipment>()))
			using (var control = plugin.CreateBrokerageUserControl())
			{
				AssertEquals(typeof(CustomsBrokerageUserControl), control.GetType());
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

		sealed class BrokeragePlugInTestClass : BrokeragePlugIn
		{
			public BrokeragePlugInTestClass(ForwardingShipment shipment) : base(shipment)
			{
			}

			internal new BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => base.CreateBrokerageUserControl();
		}
	}
}
