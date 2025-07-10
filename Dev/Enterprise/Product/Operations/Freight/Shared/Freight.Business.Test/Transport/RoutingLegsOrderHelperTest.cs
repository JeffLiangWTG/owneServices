using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Freight;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RoutingLegsOrderHelperTest : TestCaseWithFactory
	{
		#region TestFirstAndLastLegs

		public void TestFirstAndLastLegs_WithRoutingSupport()
		{
			var now = ZDateTime.Now;
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var consolTransports = consol.Transports;
			AssertEquals("Precondition", 1, consolTransports.Count);
			var firstLegFromConsol = ((ITransport)consolTransports[0]);
			AssertEquals("Precondition", (ZByte)1, firstLegFromConsol.JW_LegOrder);
			firstLegFromConsol.JW_ATA = now.AddDays(-5);
			CreateTransport(shipment.Transports, 2, now.AddDays(-4));
			CreateTransport(shipment.Transports, 3, now.AddDays(-3));
			var lastLegFromConsol = CreateTransport(consol.Transports, 4, now.AddDays(-2));
			consolTransports.Load();
			var routingOrderHelper = new RoutingLegsOrderHelper();
			routingOrderHelper.InitializeFrom(shipment);
			AssertEquals(firstLegFromConsol, routingOrderHelper.FirstLeg);
			AssertEquals(lastLegFromConsol, routingOrderHelper.LastLeg);
		}

		public void TestFirstAndLastLegs_WithTransportParentCommon()
		{
			var bookingConsolidation = (BusinessObject)Factory.New<IDtbBookingConsolidation>();

			var firstLegFromConsolidation = CreateTransport(bookingConsolidation.GetType(), bookingConsolidation.PK, 1, Constants.TransportParentTypes.TransportBooking);
			CreateTransport(bookingConsolidation.GetType(), bookingConsolidation.PK, 2, Constants.TransportParentTypes.TransportBooking);
			var lastLegFromConsolidation = CreateTransport(bookingConsolidation.GetType(), bookingConsolidation.PK, 3, Constants.TransportParentTypes.TransportBooking);

			var routingOrderHelper = new RoutingLegsOrderHelper();
			routingOrderHelper.InitializeFrom(bookingConsolidation);
			AssertEquals(firstLegFromConsolidation, routingOrderHelper.FirstLeg);
			AssertEquals(lastLegFromConsolidation, routingOrderHelper.LastLeg);
		}

		public void TestFirstAndLastLegs_NoTransports()
		{
			var routingOrderHelper = new RoutingLegsOrderHelper();
			routingOrderHelper.InitializeFrom((BusinessObject)Factory.New<IDtbBookingConsolidation>());
			AssertEquals(null, routingOrderHelper.FirstLeg);
			AssertEquals(null, routingOrderHelper.LastLeg);
		}

		#endregion

		Transport CreateTransport(TransportCollection transportCollection, ZByte legOrder, ZDateTime arrivalDate)
		{
			var transport = transportCollection.AddNew();
			transport.JW_LegOrder = legOrder;
			transport.JW_ATA = arrivalDate;
			return transport;
		}

		ITransport CreateTransport(Type parentType, ZGuid parentPK, ZByte legOrder, string transportParentType = Constants.TransportParentTypes.TransportBooking)
		{
			var transport = Factory.New<Transport>();
			transport.ParentType = parentType;
			transport.JW_ParentGUID = parentPK;
			transport.JW_LegOrder = legOrder;
			transport.JW_ParentType = transportParentType;
			return transport;
		}
	}
}
