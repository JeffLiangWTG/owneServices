//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCACCaseEventValidation
//
//    This class should be used for overriding validation in AutoUSCACCaseEventValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseEventValidation : AutoUSCACCaseEventValidation
	{
		public USCACCaseEventValidation(AutoUSCACCaseEvent parent) : base(parent)
		{
		}
	}
}
