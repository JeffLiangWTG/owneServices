//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHROnBoardingEntitlementValidation
//
//    This class should be used for overriding validation in AutoHROnBoardingEntitlementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingEntitlementValidation : AutoHROnBoardingEntitlementValidation
	{
		public HROnBoardingEntitlementValidation(AutoHROnBoardingEntitlement parent) : base(parent)
		{
		}
	}
}
