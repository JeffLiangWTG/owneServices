//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobBookedCtgMoveValidation
//
//    This class should be used for overriding validation in AutoJobBookedCtgMoveValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobBookedCtgMoveValidation : AutoJobBookedCtgMoveValidation
	{
		public JobBookedCtgMoveValidation(AutoJobBookedCtgMove parent) : base(parent)
		{
		}
	}
}
