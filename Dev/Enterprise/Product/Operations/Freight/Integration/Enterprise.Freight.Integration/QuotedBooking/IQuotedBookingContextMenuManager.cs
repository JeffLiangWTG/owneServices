namespace Enterprise.Freight.Integration.QuotedBooking
{
	public interface IQuotedBookingContextMenuManager
	{
		/// <param name="zgridInstance">
		/// Will not be anything other than a ZGrid or a subclass. Not null
		/// </param>
		/// <param name="quotedBooking">
		/// Will be a QuotedBooking. Not null
		/// </param>
		void AddMenuToCharges(object zgridInstance, IQuotedBooking quotedBooking);
	}
}
