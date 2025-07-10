using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.DocumentScanning.Testing
{
	sealed class JobApplicationDataTest : TestCaseWithFactory
	{
		public void TestHumanReadableName()
		{
			var data = new JobApplicationData();
			AssertEquals("Job Application", data.HumanReadableName.GetUnresolvedString());
		}

		public void TestOverrides()
		{
			var data = new JobApplicationData();
			AssertEquals("BusinessObjectType", typeof(HRJobApplication), data.BusinessObjectType);
			AssertEquals("Should be 'HRE' for the correct types Doc Types on eDocs", "HRE", data.ReferenceType);
		}

		public void TestShouldDisplayOneJobApplication()
		{
			var assemblyData = new JobApplicationData();

			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Software Engineer";

			var applicant = Factory.New<HRJobApplicant>();
			var application = Factory.New<HRJobApplication>();

			application.HP_HV = opening.PK;
			application.HP_HA = applicant.PK;
			application.HP_ApplicationNumber = "JA0000001";

			AssertEquals("Software Engineer JA0000001", assemblyData.GetFriendlyName(application));
		}

		public void TestShouldCorrectlyDisplayApplicationsWithNoJobOpening()
		{
			var assemblyData = new JobApplicationData();

			var openingForThisJob = Factory.New<HRRecruitmentJobCampaign>();
			openingForThisJob.HV_AdTitle = "Software Engineer";

			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var applicationForThisJob = Factory.NewWithValidTestData<HRJobApplication>();
			applicationForThisJob.HP_HV = openingForThisJob.PK;
			applicationForThisJob.HP_HA = applicant.PK;
			applicationForThisJob.HP_ApplicationNumber = "JA000200";

			var applicationForJobWithoutJobOpening = Factory.New<HRJobApplication>();
			applicationForJobWithoutJobOpening.HP_HA = applicant.PK;
			applicationForJobWithoutJobOpening.HP_ApplicationNumber = "JA000400";

			AssertEquals("Software Engineer JA000200", assemblyData.GetFriendlyName(applicationForThisJob));
			AssertEquals("No Valid Campaign JA000400", assemblyData.GetFriendlyName(applicationForJobWithoutJobOpening));
		}

		public void TestShouldDisplayMutipleJobApplications()
		{
			var assemblyData = new JobApplicationData();

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

			AssertEquals("Software Engineer JA0000001", assemblyData.GetFriendlyName(application1));
			AssertEquals("Software Engineer JA0000002", assemblyData.GetFriendlyName(application2));
			AssertEquals("Product Specialist JA0000001", assemblyData.GetFriendlyName(application3));
		}
	}
}
