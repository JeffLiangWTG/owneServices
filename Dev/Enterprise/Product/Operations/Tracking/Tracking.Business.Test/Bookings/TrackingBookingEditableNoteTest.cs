using CargoWise.EntityFramework;
using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingBookingEditableNoteTest : IWebUserEditableNoteSupportTest
	{
		protected override IWebUserEditableNoteSupport GetNewBusinessObject()
		{
			return new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null);
		}

		protected override BusinessObject ExpectedNotesParentBO
		{
			get { return ((TrackingBooking)base.ExpectedNotesParentBO).QuotedBooking.Booking; }
		}
	}
}
