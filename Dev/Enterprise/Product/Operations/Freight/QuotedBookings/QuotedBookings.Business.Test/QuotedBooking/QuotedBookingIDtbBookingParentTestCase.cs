using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBooking))]
	public class QuotedBookingIDtbBookingParentTestCase : IDtbBookingParentTestCase<QuotedBooking>
	{
		protected override QuotedBooking GetNewParent()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			return quotedBooking;
		}

		public void TestSupportedDirectionsForIsForwardRegistered()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			quotedBooking.CreateNewBookingForUniversalCopy();

			quotedBooking.Booking.JS_IsForwardRegistered = true;
			AssertEquals("Supported Directions should be empty.", 0, ((IDtbBookingParent)quotedBooking).GetSupportedDirections().Length);

			quotedBooking.Booking.JS_IsForwardRegistered = false;
			AssertEquals("Should have 1 supported direction.", 1, ((IDtbBookingParent)quotedBooking).GetSupportedDirections().Length);
			AssertEquals("Supported direction should be Pickup.", true, ((IDtbBookingParent)quotedBooking).GetSupportedDirections().Contains(DtbBookingDirection.PIC));
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking()
		{
			return false;
		}

		protected override QuotedBooking GetNewParentForJobInvoicingPluginTests()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			QuotedBooking quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			return quotedBooking;
		}

		protected override bool CanHaveDirectCartageChild => false;
	}
}
