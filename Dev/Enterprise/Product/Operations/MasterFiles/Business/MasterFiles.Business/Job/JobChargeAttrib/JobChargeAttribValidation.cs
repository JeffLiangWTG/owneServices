//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobChargeAttribValidation
//
//    This class should be used for overriding validation in AutoJobChargeAttribValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeAttribValidation : AutoJobChargeAttribValidation
	{
		public JobChargeAttribValidation(AutoJobChargeAttrib parent) : base(parent)
		{
		}
	}
}
