//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCACCaseValidation
//
//    This class should be used for overriding validation in AutoUSCACCaseValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseValidation : AutoUSCACCaseValidation
	{
		public USCACCaseValidation(AutoUSCACCase parent) : base(parent)
		{
		}
	}
}
