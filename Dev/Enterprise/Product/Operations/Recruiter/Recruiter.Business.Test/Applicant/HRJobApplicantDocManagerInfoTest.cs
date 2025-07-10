using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicantDocManagerInfo))]
	sealed class HRJobApplicantDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestRelatedObjects()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.Applications.AddNew();
			applicant.Applications.AddNew();
			applicant.Applications.AddNew();
			applicant.Applications.AddNew();
			applicant.Applications.AddNew();
			AssertEquals(5, applicant.DocManagerInfo.RelatedObjects.Length);
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<HRJobApplicant>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.Applications.AddNew();
			applicant.Applications.AddNew();
			applicant.Applications.AddNew();
			applicant.Applications.AddNew();
			applicant.Applications.AddNew();
			return applicant;
		}

		public void TestShouldContainOneJobApplication()
		{
			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Software Engineer";

			var applicant = Factory.New<HRJobApplicant>();
			var application = Factory.New<HRJobApplication>();

			application.HP_HV = opening.PK;
			application.HP_HA = applicant.PK;
			application.HP_ApplicationNumber = "JA0000001";

			AssertEquals(1, applicant.DocManagerInfo.RelatedObjects.Length);
		}

		public void TestShouldDisplayMultipleJobApplications()
		{
			var openingForJob1 = Factory.New<HRRecruitmentJobCampaign>();
			openingForJob1.HV_AdTitle = "Software Engineer";

			var openingForJob2 = Factory.New<HRRecruitmentJobCampaign>();
			openingForJob2.HV_AdTitle = "Product Specialist";

			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();

			var application1 = Factory.NewWithValidTestData<HRJobApplication>();
			application1.HP_HV = openingForJob1.PK;
			application1.HP_HA = applicant1.PK;
			application1.HP_ApplicationNumber = "JA0000001";

			var application2 = Factory.NewWithValidTestData<HRJobApplication>();
			application2.HP_HV = openingForJob1.PK;
			application2.HP_HA = applicant2.PK;
			application2.HP_ApplicationNumber = "JA0000002";

			var application3 = Factory.NewWithValidTestData<HRJobApplication>();
			application3.HP_HV = openingForJob2.PK;
			application3.HP_HA = applicant1.PK;
			application3.HP_ApplicationNumber = "JA0000001";

			AssertEquals(2, applicant1.DocManagerInfo.RelatedObjects.Length);
			AssertEquals(1, applicant2.DocManagerInfo.RelatedObjects.Length);
		}

		public void TestShouldDisplayNoRelatedeDocs()
		{
			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Software Engineer";

			var applicantWithNoApplications = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HV = opening.PK;
			application.HP_ApplicationNumber = "JA0000001";

			AssertEquals(0, applicantWithNoApplications.DocManagerInfo.RelatedObjects.Length);
		}

		public void TestShouldCorrectlyDisplayApplicationsWithNoJobOpening()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			application.HP_ApplicationNumber = "JA0000001";

			AssertEquals(1, applicant.DocManagerInfo.RelatedObjects.Length);
		}
	}
}
