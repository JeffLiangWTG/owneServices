using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.V3.GUI.Testing
{
	sealed class V3BrokeragePluginTest : TestCaseWithFactory
	{
		public void TestGetNewUserControl()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new V3BrokeragePlugin(shipment))
			{
				Assert(plugin.UserControl is V3CustomsBrokerageUserControl);
			}
		}

		public void TestName()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new V3BrokeragePlugin(shipment))
			{
				AssertEquals("Brokerage", plugin.Name);
			}
		}

		public void TestMutex()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new V3BrokeragePlugin(shipment))
			{
				AssertEquals(MutexIDs.JobBeingCreatedForShipment, plugin.Mutex.MutexID);
				AssertEquals(shipment.PK + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, plugin.Mutex.RecordIdentifier);
			}
		}
	}
}
