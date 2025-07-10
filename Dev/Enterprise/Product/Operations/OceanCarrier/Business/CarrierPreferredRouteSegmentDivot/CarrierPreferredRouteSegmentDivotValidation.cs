//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierPreferredRouteSegmentDivotValidation
//
//    This class should be used for overriding validation in AutoCarrierPreferredRouteSegmentDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierPreferredRouteSegmentDivotValidation : AutoCarrierPreferredRouteSegmentDivotValidation
	{
		public CarrierPreferredRouteSegmentDivotValidation(AutoCarrierPreferredRouteSegmentDivot parent) : base(parent)
		{
		}
	}
}
