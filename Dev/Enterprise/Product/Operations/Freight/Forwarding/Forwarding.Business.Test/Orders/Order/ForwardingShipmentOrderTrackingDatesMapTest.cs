using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentOrderTrackingDatesMap))]
	sealed class ForwardingShipmentOrderTrackingDatesMapTest : OrderTrackingDatesMapTest<ForwardingShipmentOrderTrackingDatesMap>
	{
		public override void TestDepartureActualDate()
		{
			var map = GetNewMap();

			AssertEquals("DepartureActualDate", actualDepartureDate, map.DepartureActualDate);
		}

		public override void TestArrivalActualDate()
		{
			var map = GetNewMap();

			AssertEquals("ArrivalActualDate", actualArrivalDate, map.ArrivalActualDate);
		}

		public override void TestCargoAvailableActualDate()
		{
			var map = GetNewMap();

			AssertEquals("CargoAvailableActualDate", fclAvailable, map.CargoAvailableActualDate);
		}

		public override void TestDeliveryCartageAdvisedActualDate()
		{
			var map = GetNewMap();

			AssertEquals("DeliveryCartageAdvisedActualDate", deliveryCartageAdvised, map.DeliveryCartageAdvisedActualDate);
		}

		public override void TestDeliveryCartageCompleteFinalizedActualDate()
		{
			var map = GetNewMap();

			AssertEquals("DeliveryCartageCompleteFinalizedActualDate", deliveryCartageCompleted, map.DeliveryCartageCompleteFinalizedActualDate);
		}

		public override void TestDepartureScheduledDate()
		{
			var map = GetNewMap();

			AssertEquals("DepartureScheduledDate", estimatedDepartureDate, map.DepartureScheduledDate);
		}

		public override void TestArrivalScheduledDate()
		{
			var map = GetNewMap();

			AssertEquals("ArrivalScheduledDate", estimatedArrivalDate, map.ArrivalScheduledDate);
		}

		public override void TestDeliveryCartageCompleteFinalizedScheduledDate()
		{
			var map = GetNewMap();

			AssertEquals("DeliveryCartageCompleteFinalizedScheduledDate", estimatedCartageDelivery, map.DeliveryCartageCompleteFinalizedScheduledDate);
		}

		readonly ZDateTime estimatedDepartureDate = new ZDateTime(2012, 12, 31);
		readonly ZDateTime actualDepartureDate = new ZDateTime(2013, 1, 1);

		readonly ZDateTime estimatedArrivalDate = new ZDateTime(2012, 1, 9);
		readonly ZDateTime actualArrivalDate = new ZDateTime(2013, 1, 10);

		readonly ZDateTime estimatedCartageDelivery = new ZDateTime(2013, 1, 11);
		readonly ZDateTime fclAvailable = new ZDateTime(2013, 1, 12);
		readonly ZDateTime deliveryCartageAdvised = new ZDateTime(2013, 1, 13);
		readonly ZDateTime deliveryCartageCompleted = new ZDateTime(2013, 1, 14);

		ForwardingShipmentOrderTrackingDatesMap GetNewMap()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USSFO";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var departureTransport = shipment.Transports.AddNew();
			departureTransport.JW_RL_NKLoadPort = "AUSYD";
			departureTransport.JW_RL_NKDiscPort = "NZAKL";
			departureTransport.JW_ETD = estimatedDepartureDate;
			departureTransport.JW_ATD = actualDepartureDate;

			var arrivalTransport = shipment.Transports.AddNew();
			arrivalTransport.JW_RL_NKLoadPort = "NZAKL";
			arrivalTransport.JW_RL_NKDiscPort = "USSFO";
			arrivalTransport.JW_ETA = estimatedArrivalDate;
			arrivalTransport.JW_ATA = actualArrivalDate;

			shipment.DocsAndCartage.JP_FCLAvailable = fclAvailable;
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = deliveryCartageAdvised;
			shipment.DocsAndCartage.JP_EstimatedDelivery = estimatedCartageDelivery;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = deliveryCartageCompleted;

			return new ForwardingShipmentOrderTrackingDatesMap(shipment);
		}
	}
}
