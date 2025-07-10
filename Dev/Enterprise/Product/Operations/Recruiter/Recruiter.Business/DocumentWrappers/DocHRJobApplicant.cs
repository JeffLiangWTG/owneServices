using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class DocHRJobApplicant : DocRecruiterBase
	{
		DocHRJobApplicant(HRJobApplicant applicant, BusinessObjectFactory factory)
			: base(applicant, factory)
		{
		}

		public static DocHRJobApplicant New(HRJobApplicant applicant, BusinessObjectFactory factory)
		{
			return new DocHRJobApplicant(applicant, factory);
		}

		public new HRJobApplicant WrappedObject
		{
			get { return (HRJobApplicant)base.WrappedObject; }
		}

		protected override HRJobApplicant Applicant
		{
			get { return WrappedObject; }
		}
	}
}
