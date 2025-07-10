//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCACCaseRateValidation
//
//    This class should be used for overriding validation in AutoUSCACCaseRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseRateValidation : AutoUSCACCaseRateValidation
	{
		public USCACCaseRateValidation(AutoUSCACCaseRate parent) : base(parent)
		{
		}
	}
}
