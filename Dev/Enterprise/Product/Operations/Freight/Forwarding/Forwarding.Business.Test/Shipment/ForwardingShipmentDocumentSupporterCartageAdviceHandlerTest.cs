using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentDocumentSupporterCartageAdviceHandlerTest : DocumentSupporterCartageAdviceHandlerTest
	{
		protected override IDocumentSupportable GetDocumentSupporterParent()
		{
			var shipment = GetShipment();

			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = shipment.TablePrefix;
			consolidation.KB_JobDirection = ((IDtbBookingParent)shipment).GetSupportedDirections()[0].ToString();

			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;

			return shipment;
		}

		ForwardingShipment GetShipment()
		{
			return Factory.New<ForwardingShipment>();
		}

		protected override IDocumentSupportable[] GetBookingsFromBookingsProperty(DocumentSupporter documentSupporter)
		{
			return ((ForwardingShipmentDocumentSupporter)documentSupporter).Bookings;
		}

		protected override bool ChecksChildMenuItem => true;
		protected override bool DocumentSupportablesUseDifferentFactory => true;
	}
}
