using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business.Test
{
	public class DtbBookingConsolRatingAdaptersProviderTest : TestCaseWithFactory
	{
		public void TestIJobInvoicingHostWithAdditionalJobs_AdditionalJobs()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var provider = ((IRatingSupporter)bookingConsolidation).AdaptersProvider;
			AssertEquals(0, provider.GetAdditionalJobs().Count);

			var booking1 = bookingConsolidation.Bookings.AddNew();
			var booking2 = bookingConsolidation.Bookings.AddNew();
			var randomBooking = Factory.New<DtbBooking>();
			AssertContainsExactElementsInAnyOrder(new[] { booking1, booking2 }, provider.GetAdditionalJobs());
		}
	}
}
