using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationInterview))]
	sealed class HRJobApplicationInterviewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetCustomLogReference()
		{
			GlbStaff garyStaff = Factory.NewWithValidTestData<GlbStaff>();
			garyStaff.GS_FullName = "Gary";

			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			Factory.Save();
			applicant.HA_FullName = "Bob";
			HRJobApplication application = applicant.Applications.AddNew();
			application.HP_HV = campaign.PK;

			HRJobApplicationInterview interview = application.Interviews.AddNew();
			interview.HI_GS_NKInterviewer = garyStaff.GS_Code;

			Factory.Save();

			AssertEquals("Should contain 1 log", 1, interview.Logs.GetAllLogs().Count);
			AssertEquals("Log text should be", "Interview for Bob with Gary", interview.Logs.GetAllLogs()[0].SL_Reference);
		}

		public void TestApplication()
		{
			HRJobApplication application = Factory.New<HRJobApplication>();
			HRJobApplicationInterview interview = application.Interviews.AddNew();

			AssertEquals("Application should be accessible through Interview", application, interview.Application);
		}

		public void TestIsAutoLogged()
		{
			HRJobApplicationInterview interview = Factory.NewWithValidTestData<HRJobApplicationInterview>();
			Factory.Save();

			AssertEquals("One log should exist", 1, interview.Logs.GetAllLogs().Count);
		}

		public void TestFillWithValidTestDataCore()
		{
			HRJobApplicationInterview interview = Factory.NewWithValidTestData<HRJobApplicationInterview>();
			Assert("Interview should be for a valid Application", interview.HI_HP.IsValid);
		}
	}
}
