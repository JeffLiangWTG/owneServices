using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationDocManagerInfo))]
	sealed class HRJobApplicationDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestRelatedObjects()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			AssertEquals(1, application.DocManagerInfo.RelatedObjects.Length);
			AssertEquals(typeof(HRJobApplicant), application.DocManagerInfo.RelatedObjects[0].GetType());
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<HRJobApplication>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			return application;
		}

		public void TestShouldOnlyDisplayThisJobApplicationOfJobApplicant()
		{
			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Software Engineer";

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HV = opening.PK;
			application.HP_HA = applicant.PK;
			application.HP_ApplicationNumber = "JA1000000";

			AssertEquals(1, application.DocManagerInfo.RelatedObjects.Length);
		}

		public void TestShouldDisplayMultipleJobApplicationsOfJobApplicant()
		{
			var openingForThisJob = Factory.New<HRRecruitmentJobCampaign>();
			openingForThisJob.HV_AdTitle = "Software Engineer";

			var openingForJob2 = Factory.New<HRRecruitmentJobCampaign>();
			openingForJob2.HV_AdTitle = "Product Specialist";

			var openingForJob3 = Factory.New<HRRecruitmentJobCampaign>();
			openingForJob3.HV_AdTitle = "Chef";

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var applicationForThisJob = Factory.NewWithValidTestData<HRJobApplication>();
			applicationForThisJob.HP_HV = openingForThisJob.PK;
			applicationForThisJob.HP_HA = applicant.PK;
			applicationForThisJob.HP_ApplicationNumber = "JA1234567";

			var applicationForJob2 = Factory.NewWithValidTestData<HRJobApplication>();
			applicationForJob2.HP_HV = openingForJob2.PK;
			applicationForJob2.HP_HA = applicant.PK;
			applicationForJob2.HP_ApplicationNumber = "JA0000001";

			var applicationForJob3 = Factory.NewWithValidTestData<HRJobApplication>();
			applicationForJob3.HP_HV = openingForJob3.PK;
			applicationForJob3.HP_HA = applicant.PK;
			applicationForJob3.HP_ApplicationNumber = "JA0000001";

			AssertEquals(3, applicationForThisJob.DocManagerInfo.RelatedObjects.Length);
		}

		public void TestShouldCorrectlyDisplayApplicationsWithNoJobOpening()
		{
			var openingForThisJob = Factory.New<HRRecruitmentJobCampaign>();
			openingForThisJob.HV_AdTitle = "Software Engineer";

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var applicationForThisJob = Factory.NewWithValidTestData<HRJobApplication>();
			applicationForThisJob.HP_HV = openingForThisJob.PK;
			applicationForThisJob.HP_HA = applicant.PK;
			applicationForThisJob.HP_ApplicationNumber = "JA000200";

			var applicationForJobWithoutJobOpening = Factory.NewWithValidTestData<HRJobApplication>();
			applicationForJobWithoutJobOpening.HP_HA = applicant.PK;
			applicationForJobWithoutJobOpening.HP_ApplicationNumber = "JA0000001";

			AssertEquals(2, applicationForThisJob.DocManagerInfo.RelatedObjects.Length);
		}
	}
}
