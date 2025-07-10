//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierVoyageValidation
//
//    This class should be used for overriding validation in AutoCarrierVoyageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyageValidation : AutoCarrierVoyageValidation
	{
		public CarrierVoyageValidation(AutoCarrierVoyage parent) : base(parent)
		{
		}
	}
}
