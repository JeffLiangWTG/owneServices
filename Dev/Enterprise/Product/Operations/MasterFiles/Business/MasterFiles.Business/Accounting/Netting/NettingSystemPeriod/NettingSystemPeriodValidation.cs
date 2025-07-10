//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingSystemPeriodValidation
//
//    This class should be used for overriding validation in AutoNettingSystemPeriodValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class NettingSystemPeriodValidation : AutoNettingSystemPeriodValidation
	{
		public NettingSystemPeriodValidation(AutoNettingSystemPeriod parent) : base(parent)
		{
		}
	}
}
