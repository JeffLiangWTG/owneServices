//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierVoyagePortCallValidation
//
//    This class should be used for overriding validation in AutoCarrierVoyagePortCallValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallValidation : AutoCarrierVoyagePortCallValidation
	{
		public CarrierVoyagePortCallValidation(AutoCarrierVoyagePortCall parent) : base(parent)
		{
		}
	}
}
