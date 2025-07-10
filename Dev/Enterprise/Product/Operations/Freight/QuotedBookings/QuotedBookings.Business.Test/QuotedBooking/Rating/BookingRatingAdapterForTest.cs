using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	class BookingRatingAdapterForTest : BookingRatingAdapter
	{
		public BookingRatingAdapterForTest(QuotedBooking quotedBooking)
			: base(quotedBooking)
		{
		}

		public JobHeader ParentJobForTest
		{
			get { return base.ParentJob; }
		}
	}
}
