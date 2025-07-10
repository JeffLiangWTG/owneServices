//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHRJobApplicationInterviewValidation
//
//    This class should be used for overriding validation in AutoHRJobApplicationInterviewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationInterviewValidation : AutoHRJobApplicationInterviewValidation
	{
		public HRJobApplicationInterviewValidation(AutoHRJobApplicationInterview parent) : base(parent)
		{
		}

		protected override void CheckHI_GS_NKInterviewer()
		{
			base.CheckHI_GS_NKInterviewer();
			MandatoryValidation.CheckEntered(Parent.HI_GS_NKInterviewerInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HI_GS_NKInterviewerInfo, Parent.Lookups.Interviewers);
		}

		protected override void CheckHI_InterviewStatus()
		{
			base.CheckHI_InterviewStatus();
			ListValidation.ErrorIfInvalidCode(Parent.HI_InterviewStatusInfo, Parent.Lookups.InterviewStatuses);
		}
	}
}
