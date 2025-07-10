namespace Enterprise.Freight.Integration.QuotedBooking
{
	public enum QuotedBookingState
	{
		None = 0,
		QuoteOnly = 1,
		BookingOnly = 2,
		AcceptedBookingWithQuote = 3,
		UnacceptedBookingWithQuote = 4,
	}
}
