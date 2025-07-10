namespace Enterprise.Freight.Integration.QuotedBooking
{
	public interface IQuotedBookingController
	{
		void SetQuotedBookingState(QuotedBookingState state);
		bool SkipRecentItems { set; }
	}
}
