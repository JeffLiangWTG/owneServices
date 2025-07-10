using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class PortMessagingHelperTest : TestCaseWithFactory
	{
		public void TestCheckPortMessaging()
		{
			Func<IPortMessaging, bool> predicate = x => x.EntryType == "XYZ";

			var consol = Factory.New<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			var packLine = shipment.OuterPackLines.AddNew();
			var packLinePortMessaging = PackLinePortMessaging.LoadOrCreate(packLine);

			packLinePortMessaging.JLM_EntryType = "XYZ";
			AssertEquals(true, PortMessagingHelper.CheckPortMessagingForConsol(consol, predicate));
			AssertEquals(true, PortMessagingHelper.CheckPortMessagingForShipment(shipment, predicate));
			AssertEquals(true, PortMessagingHelper.CheckPortMessagingForPackLine(packLine, predicate));

			packLinePortMessaging.JLM_EntryType = "ABC";
			AssertEquals(false, PortMessagingHelper.CheckPortMessagingForConsol(consol, predicate));
			AssertEquals(false, PortMessagingHelper.CheckPortMessagingForShipment(shipment, predicate));
			AssertEquals(false, PortMessagingHelper.CheckPortMessagingForPackLine(packLine, predicate));

			shipmentPortMessaging.JSM_EntryType = "XYZ";
			AssertEquals(true, PortMessagingHelper.CheckPortMessagingForConsol(consol, predicate));
			AssertEquals(true, PortMessagingHelper.CheckPortMessagingForShipment(shipment, predicate));
			AssertEquals(false, PortMessagingHelper.CheckPortMessagingForPackLine(packLine, predicate));

			shipmentPortMessaging.JSM_EntryType = "ABC";
			AssertEquals(false, PortMessagingHelper.CheckPortMessagingForConsol(consol, predicate));
			AssertEquals(false, PortMessagingHelper.CheckPortMessagingForShipment(shipment, predicate));
			AssertEquals(false, PortMessagingHelper.CheckPortMessagingForPackLine(packLine, predicate));
		}
	}
}
