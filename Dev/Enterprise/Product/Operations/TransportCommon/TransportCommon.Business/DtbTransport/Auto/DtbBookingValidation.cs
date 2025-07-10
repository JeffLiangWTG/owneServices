//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbBookingValidation
//
//    This class should be used for overriding validation in AutoDtbBookingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportCommon.Business.Common
{
	public class DtbBookingValidation : AutoDtbBookingValidation
	{
		internal DtbBookingValidation(AutoDtbBooking parent)
			: base(parent)
		{
		}
	}
}
