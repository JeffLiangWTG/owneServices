//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierVoyagePortCallDivotValidation
//
//    This class should be used for overriding validation in AutoCarrierVoyagePortCallDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallDivotValidation : AutoCarrierVoyagePortCallDivotValidation
	{
		public CarrierVoyagePortCallDivotValidation(AutoCarrierVoyagePortCallDivot parent) : base(parent)
		{
		}
	}
}
