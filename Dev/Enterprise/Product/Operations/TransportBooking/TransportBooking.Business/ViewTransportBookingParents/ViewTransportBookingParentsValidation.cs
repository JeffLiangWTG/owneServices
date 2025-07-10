//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewTransportBookingParentsValidation
//
//    This class should be used for overriding validation in AutoViewTransportBookingParentsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportBookings.Business
{
	public class ViewTransportBookingParentsValidation : AutoViewTransportBookingParentsValidation
	{
		public ViewTransportBookingParentsValidation(AutoViewTransportBookingParents parent) : base(parent)
		{
		}
	}
}
