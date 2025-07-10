//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHROnBoardingLookups
//
//    This class should be used for overriding collections in AutoHROnBoardingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingLookups : AutoHROnBoardingLookups
	{
		public HROnBoardingLookups(AutoHROnBoarding parent) : base(parent)
		{
		}

		#region Applicants

		public HRJobApplicantCollection Applicants => fApplicants ?? (fApplicants = new HRJobApplicantCollection(Factory));

		HRJobApplicantCollection fApplicants;

		#endregion
	}
}
