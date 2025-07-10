using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbAgentBookingInvoicingSupporter))]
	class DtbAgentBookingInvoicingSupporterTest : DtbBookingInvoicingSupporterTest
	{
		public override void TestConsumerType()
		{
			AssertEquals(JobInvoicingConsumerTypes.AgentBooking, InvoicingSupporter.ConsumerType);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var booking = (DtbBooking)base.GetNewBusinessObject();
			DtbAgentBooking.NewFromBooking(booking);
			return booking;
		}

		protected override DtbBookingInvoicingSupporter CreateNewInvoicingSupporter(DtbBooking booking)
		{
			return new DtbAgentBookingInvoicingSupporter(booking.AgentBooking);
		}
	}
}
