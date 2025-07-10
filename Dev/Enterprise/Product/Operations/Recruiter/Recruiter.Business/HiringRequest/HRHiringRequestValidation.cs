//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHRHiringRequestValidation
//
//    This class should be used for overriding validation in AutoHRHiringRequestValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class HRHiringRequestValidation : AutoHRHiringRequestValidation
	{
		public HRHiringRequestValidation(AutoHRHiringRequest parent) : base(parent)
		{
		}
	}
}
