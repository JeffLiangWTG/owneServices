//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConsolidatedTransportBookingValidation
//
//    This class should be used for overriding validation in AutoJobConsolidatedTransportBookingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobConsolidatedTransportBookingValidation : AutoJobConsolidatedTransportBookingValidation
	{
		public JobConsolidatedTransportBookingValidation(AutoJobConsolidatedTransportBooking parent) : base(parent)
		{
		}
	}
}
