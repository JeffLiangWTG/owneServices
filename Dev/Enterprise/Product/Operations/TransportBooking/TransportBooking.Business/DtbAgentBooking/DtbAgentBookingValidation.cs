//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbAgentBookingValidation
//
//    This class should be used for overriding validation in AutoDtbAgentBookingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportBookings.Business
{
	public class DtbAgentBookingValidation : AutoDtbAgentBookingValidation
	{
		public DtbAgentBookingValidation(AutoDtbAgentBooking parent)
			: base(parent)
		{
		}
	}
}

