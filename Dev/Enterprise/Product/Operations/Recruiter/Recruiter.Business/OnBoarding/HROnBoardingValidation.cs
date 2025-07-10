//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHROnBoardingValidation
//
//    This class should be used for overriding validation in AutoHROnBoardingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingValidation : AutoHROnBoardingValidation
	{
		public HROnBoardingValidation(AutoHROnBoarding parent) : base(parent)
		{
		}
	}
}
