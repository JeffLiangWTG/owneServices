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
	public class JobSupplierBookingLineValidation : AutoJobSupplierBookingLineValidation
	{
		public JobSupplierBookingLineValidation(AutoJobSupplierBookingLine parent) : base(parent)
		{
		}
	}
}
