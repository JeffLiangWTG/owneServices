using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class PortMessagingForwardingHelperTest : TestCaseWithFactory
	{
		public void TestSyncronise()
		{
			var shipment = Factory.New<ForwardingShipment>();

			new PortMessagingForwardingHelper().SyncroniseMRN(Factory, shipment.PK, "", "11");
			var portMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			AssertEquals("Value was syncronised", "11", portMessaging.JSM_MovementReferenceNumber);

			new PortMessagingForwardingHelper().SyncroniseMRN(Factory, shipment.PK, "11", "22");
			AssertEquals("Value was syncronised", "22", portMessaging.JSM_MovementReferenceNumber);

			new PortMessagingForwardingHelper().SyncroniseMRN(Factory, shipment.PK, "FOO", "33");
			AssertEquals("Value wasn't syncronised - different initial MRN's", "22", portMessaging.JSM_MovementReferenceNumber);
		}

		public void TestOnPacklineDelete()
		{
			var packline = Factory.New<ForwardingPackLine>();
			var portMessaging = PackLinePortMessaging.LoadOrCreate(packline);

			new PortMessagingForwardingHelper().OnPacklineDelete(Factory, packline.PK);
			AssertEquals("Port messaging was deleted", true, portMessaging.IsDeleted);

			new PortMessagingForwardingHelper().OnPacklineDelete(Factory, packline.PK);
			AssertEquals(true, portMessaging.IsDeleted);
		}
	}
}
