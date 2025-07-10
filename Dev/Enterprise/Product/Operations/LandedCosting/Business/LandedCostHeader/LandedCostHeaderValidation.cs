//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLandedCostHeaderValidation
//
//    This class should be used for overriding validation in AutoLandedCostHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostHeaderValidation : AutoLandedCostHeaderValidation
	{
		public LandedCostHeaderValidation(AutoLandedCostHeader parent) : base(parent)
		{
		}
	}
}
