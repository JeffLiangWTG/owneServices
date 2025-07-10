//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobChargeTargetValidation
//
//    This class should be used for overriding validation in AutoJobChargeTargetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeTargetValidation : AutoJobChargeTargetValidation
	{
		public JobChargeTargetValidation(AutoJobChargeTarget garent) : base(garent)
		{
		}
	}
}
