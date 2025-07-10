//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewTransportBookingParentLegsValidation
//
//    This class should be used for overriding validation in AutoViewTransportBookingParentLegsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportBookings.Business
{
	public class ViewTransportBookingParentLegsValidation : AutoViewTransportBookingParentLegsValidation
	{
		public ViewTransportBookingParentLegsValidation(AutoViewTransportBookingParentLegs parent)
			: base(parent) { }
	}
}
