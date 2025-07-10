using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationInterviewLookups : AutoHRJobApplicationInterviewLookups
	{
		public HRJobApplicationInterviewLookups(AutoHRJobApplicationInterview parent) : base(parent)
		{
		}

		#region  InterviewStatuses

		public ReadOnlyCodeDescriptionPairList InterviewStatuses
		{
			get { return RecruiterDataRegistry.Instance.InterviewStatuses.Value; }
		}

		#endregion
	}
}
