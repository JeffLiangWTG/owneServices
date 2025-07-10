//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLandedLineCostItemValidation
//
//    This class should be used for overriding validation in AutoLandedLineCostItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.LandedCosting.Business
{
	public class LandedLineCostItemValidation : AutoLandedLineCostItemValidation
	{
		public LandedLineCostItemValidation(AutoLandedLineCostItem parent) : base(parent)
		{
		}
	}
}
