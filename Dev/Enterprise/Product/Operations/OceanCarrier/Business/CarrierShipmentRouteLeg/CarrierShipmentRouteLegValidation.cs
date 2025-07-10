//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierShipmentRouteLegValidation
//
//    This class should be used for overriding validation in AutoCarrierShipmentRouteLegValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentRouteLegValidation : AutoCarrierShipmentRouteLegValidation
	{
		public CarrierShipmentRouteLegValidation(AutoCarrierShipmentRouteLeg parent) : base(parent)
		{
		}
	}
}
