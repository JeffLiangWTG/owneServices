using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationDependentCollection))]
	sealed class HRJobApplicationDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<HRJobApplicationDependentCollection>
	{
		protected override HRJobApplicationDependentCollection GetCollectionToTest()
		{
			HRJobApplicant parent = Factory.New<HRJobApplicant>();
			return new HRJobApplicationDependentCollection(parent);
		}

		[ExpectNoExceptions()]
		public void TestParent()
		{
			var campaign = Factory.New<HRRecruitmentJobCampaign>();
			var applicationCollectionForCampaign = new HRJobApplicationDependentCollection(campaign);

			var applicant = Factory.New<HRJobApplicant>();
			var applicationCollectionForApplicant = new HRJobApplicationDependentCollection(applicant);
		}

		public void TestLoadFindsRelatedObjects()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;

			var collection = new HRJobApplicationDependentCollection(applicant);
			AssertEquals(application, collection.Single());
		}
	}
}
