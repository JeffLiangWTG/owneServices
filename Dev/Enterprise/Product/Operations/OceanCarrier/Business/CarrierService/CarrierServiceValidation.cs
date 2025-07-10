//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierServiceValidation
//
//    This class should be used for overriding validation in AutoCarrierServiceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierServiceValidation : AutoCarrierServiceValidation
	{
		public CarrierServiceValidation(AutoCarrierService parent) : base(parent)
		{
		}
	}
}

