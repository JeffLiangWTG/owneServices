//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCarrierVesselPivotValidation
//
//    This class should be used for overriding validation in AutoRefCarrierVesselPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCarrierVesselPivotValidation : AutoRefCarrierVesselPivotValidation
	{
		public RefCarrierVesselPivotValidation(AutoRefCarrierVesselPivot parent) : base(parent)
		{
		}
	}
}
