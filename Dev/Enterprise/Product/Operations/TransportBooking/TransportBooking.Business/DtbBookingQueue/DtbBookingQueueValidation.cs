//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbBookingQueueValidation
//
//    This class should be used for overriding validation in AutoDtbBookingQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingQueueValidation : AutoDtbBookingQueueValidation
	{
		public DtbBookingQueueValidation(AutoDtbBookingQueue parent) : base(parent)
		{
		}
	}
}
