using Enterprise.DocumentEngine.GUI.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingBookingDocumentsMenuProviderTest : DocumentsMenuProviderTest
	{
		public void TestIDocumentsMenuProvider()
		{
			TrackingBooking booking = new TrackingBooking(Factory, null);
			AssertIDocumentsMenuProvider(booking, "TrackingBooking");
		}
	}
}
