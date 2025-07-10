//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSupplierBookingHeaderValidation
//
//    This class should be used for overriding validation in AutoSupplierBookingHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.eManifest.Business
{
	public class SupplierBookingHeaderValidation : AutoSupplierBookingHeaderValidation
	{
		public SupplierBookingHeaderValidation(AutoSupplierBookingHeader parent)
			: base(parent)
		{
		}
	}
}

