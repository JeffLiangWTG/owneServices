//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewQuotedBookingValidation
//
//    This class should be used for overriding validation in AutoViewQuotedBookingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ViewQuotedBookingValidation : AutoViewQuotedBookingValidation
	{
		public ViewQuotedBookingValidation(AutoViewQuotedBooking parent) : base(parent)
		{
		}
	}
}
