//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHRJobApplicationParsingQueueValidation
//
//    This class should be used for overriding validation in AutoHRJobApplicationParsingQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationParsingQueueValidation : AutoHRJobApplicationParsingQueueValidation
	{
		public HRJobApplicationParsingQueueValidation(AutoHRJobApplicationParsingQueue parent) : base(parent)
		{
		}
	}
}
