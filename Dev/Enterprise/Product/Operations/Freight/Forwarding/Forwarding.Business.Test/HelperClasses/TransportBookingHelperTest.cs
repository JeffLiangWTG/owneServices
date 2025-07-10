using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class TransportBookingHelperTest : TestCaseWithFactory
	{
		IDtbBooking CreateBooking(ForwardingShipment shipment, DtbBookingDirection direction, bool isActive)
		{
			var sConsolidation = (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			sConsolidation.KB_ParentTableCode = shipment.TablePrefix;
			sConsolidation.KB_ParentID = shipment.PK;
			sConsolidation.KB_JobDirection = direction.ToString();

			var sBooking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			sBooking.KM_KB_Booking = sConsolidation.PK;
			var booking = (sBooking as ICancellable);

			booking.IsCancelled = !isActive;
			return sBooking;
		}

		public void TestTransportBookingHelper_HasTransportBookingDLV()
		{
			// Arrange
			var shipment = Factory.New<ForwardingShipment>();

			// Act
			CreateBooking(shipment, DtbBookingDirection.DLV, false);
			CreateBooking(shipment, DtbBookingDirection.PIC, true);

			// Assert
			Assert("HasTransportBooking(DLV) is false with no active DLV booking", !shipment.HasTransportBooking(DtbBookingDirection.DLV));

			// Arrange
			var shipment2 = Factory.New<ForwardingShipment>();

			// Act
			CreateBooking(shipment2, DtbBookingDirection.DLV, true);
			CreateBooking(shipment2, DtbBookingDirection.DLV, false);

			// Assert
			Assert("HasTransportBooking(DLV) is true with active DLV booking", shipment2.HasTransportBooking(DtbBookingDirection.DLV));
		}

		public void TestTransportBookingHelper_HasTransportBookingPIC()
		{
			// Arrange
			var shipment = Factory.New<ForwardingShipment>();

			// Act
			CreateBooking(shipment, DtbBookingDirection.PIC, false);
			CreateBooking(shipment, DtbBookingDirection.DLV, true);

			// Assert
			Assert("HasTransportBooking(PIC) is false with no active PIC booking", !shipment.HasTransportBooking(DtbBookingDirection.PIC));

			// Arrange
			var shipment2 = Factory.New<ForwardingShipment>();

			// Act
			CreateBooking(shipment, DtbBookingDirection.PIC, true);
			CreateBooking(shipment, DtbBookingDirection.PIC, false);

			// Assert
			Assert("HasTransportBooking(PIC) is true with active PIC booking", !shipment2.HasTransportBooking(DtbBookingDirection.PIC));
		}

		public void TestTransportBookingHelper_GetTransportBookingsFiltersOutInactiveBookings()
		{
			// Arrange
			var shipment = Factory.New<ForwardingShipment>();

			// Assert
			AssertEquals("GetTransportBookings(DLV).Count is 0", 0, shipment.GetTransportBookings(DtbBookingDirection.DLV).Count());
			AssertEquals("GetTransportBookings(PIC).Count is 0", 0, shipment.GetTransportBookings(DtbBookingDirection.PIC).Count());
			AssertEquals("GetTransportBookings().Count is 0", 0, shipment.GetTransportBookings().Count());

			// Act
			var booking1 = CreateBooking(shipment, DtbBookingDirection.DLV, true);

			// Assert
			AssertEquals("GetTransportBookings(DLV).Count is 1", 1, shipment.GetTransportBookings(DtbBookingDirection.DLV).Count());
			AssertEquals("GetTransportBookings(PIC).Count is 0", 0, shipment.GetTransportBookings(DtbBookingDirection.PIC).Count());
			AssertEquals("GetTransportBookings().Count is 1", 1, shipment.GetTransportBookings().Count());
			AssertEquals(booking1, shipment.GetTransportBookings(DtbBookingDirection.DLV).ToList()[0]);
			AssertEquals(booking1, shipment.GetTransportBookings().ToList()[0]);

			// Act
			var booking2 = CreateBooking(shipment, DtbBookingDirection.PIC, true);

			// Assert
			AssertEquals("GetTransportBookings(DLV).Count is 1", 1, shipment.GetTransportBookings(DtbBookingDirection.DLV).Count());
			AssertEquals("GetTransportBookings(PIC).Count is 1", 1, shipment.GetTransportBookings(DtbBookingDirection.PIC).Count());
			AssertEquals("GetTransportBookings().Count is 2", 2, shipment.GetTransportBookings().Count());
			AssertEquals(booking2, shipment.GetTransportBookings(DtbBookingDirection.PIC).ToList()[0]);
			AssertEquals(booking2, shipment.GetTransportBookings().ToList()[1]);

			// Act
			CreateBooking(shipment, DtbBookingDirection.DLV, false);
			CreateBooking(shipment, DtbBookingDirection.PIC, false);

			// Assert
			AssertEquals("GetTransportBookings(DLV).Count is 1", 1, shipment.GetTransportBookings(DtbBookingDirection.DLV).Count());
			AssertEquals("GetTransportBookings(PIC).Count is 1", 1, shipment.GetTransportBookings(DtbBookingDirection.PIC).Count());
			AssertEquals("GetTransportBookings().Count is 2", 2, shipment.GetTransportBookings().Count());

			// Act
			var booking3 = CreateBooking(shipment, DtbBookingDirection.DLV, true);
			var booking4 = CreateBooking(shipment, DtbBookingDirection.PIC, true);

			// Assert
			AssertEquals("GetTransportBookings(DLV).Count is 2", 2, shipment.GetTransportBookings(DtbBookingDirection.DLV).Count());
			AssertEquals("GetTransportBookings(PIC).Count is 2", 2, shipment.GetTransportBookings(DtbBookingDirection.PIC).Count());
			AssertEquals("GetTransportBookings().Count is 4", 4, shipment.GetTransportBookings().Count());

			AssertEquals(booking3, shipment.GetTransportBookings(DtbBookingDirection.DLV).ToList()[1]);
			AssertEquals(booking4, shipment.GetTransportBookings(DtbBookingDirection.PIC).ToList()[1]);

			AssertEquals(booking3, shipment.GetTransportBookings().ToList()[2]);
			AssertEquals(booking4, shipment.GetTransportBookings().ToList()[3]);
		}
	}
}
