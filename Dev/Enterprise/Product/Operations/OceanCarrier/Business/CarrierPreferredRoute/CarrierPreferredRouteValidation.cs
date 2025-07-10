//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierPreferredRouteValidation
//
//    This class should be used for overriding validation in AutoCarrierPreferredRouteValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierPreferredRouteValidation : AutoCarrierPreferredRouteValidation
	{
		public CarrierPreferredRouteValidation(AutoCarrierPreferredRoute parent) : base(parent)
		{
		}
	}
}
