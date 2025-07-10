//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobSupplierBookingValidation
//
//    This class should be used for overriding validation in AutoJobSupplierBookingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobSupplierBookingValidation : AutoJobSupplierBookingValidation
	{
		public JobSupplierBookingValidation(AutoJobSupplierBooking parent) : base(parent)
		{
		}
	}
}
