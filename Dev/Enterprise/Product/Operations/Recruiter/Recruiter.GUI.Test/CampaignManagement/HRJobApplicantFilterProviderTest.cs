using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobApplicantFilterProvider))]
	public class HRJobApplicantFilterProviderTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation
		protected HRJobApplicant Applicant1, Applicant2;
		protected HRJobApplicantCollection Collection;
		protected override void SetUp()
		{
			base.SetUp();
			Collection = new HRJobApplicantCollection(Factory);
			Applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			Applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HRJobApplicantFilterProvider();
		}
		#endregion
	}
}
