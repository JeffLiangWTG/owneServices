using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationInterviewDependentCollection : DependentBusinessObjectCollection<HRJobApplicationInterview, HRJobApplication>
	{
		public HRJobApplicationInterviewDependentCollection(HRJobApplication parent) : base(parent)
		{
		}
	}
}
