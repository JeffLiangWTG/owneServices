using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingBookingModifiedEventHelperTest : BusinessObjectModifiedEventHelperTest
	{
		protected override BusinessObject GetNewTestEventReferenceProvider()
		{
			var booking = new TrackingBooking(Factory, Helper.TestSiteUser);
			booking.UserEditableNoteHelper.EditableNoteText = "Test";
			return booking;
		}
	}
}
