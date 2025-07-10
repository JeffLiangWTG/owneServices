using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class DocHRJobApplicationTest : DocRecruiterTestCase<DocHRJobApplication>
	{
		public void TestProperties()
		{
			DocHRJobApplication wrapper = GetDocumentWrapper();
			AssertEquals("Ad Title", wrapper.AdTitle);
			AssertEquals("Beverage Manager", wrapper.JobRole);
			AssertEquals(new ZDate(2008, 2, 1), wrapper.AdStartDate);
			AssertEquals(new ZDate(2008, 3, 1), wrapper.AdEndDate);
			AssertEquals((ZByte)3, wrapper.NoOfPositionsAvailable);
			AssertEquals((ZByte)1, wrapper.NoOfPositionsFilled);
			AssertEquals("HR Manager", wrapper.RecruiterContactName);
			AssertEquals("AUD 10,000", wrapper.SalaryRange);
		}

		protected override DocHRJobApplication GetDocumentWrapper()
		{
			return DocHRJobApplication.New(application, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			role.HJ_JobTitle = "Beverage Manager";

			campaign.HV_AdTitle = "Ad Title";
			campaign.HV_HJ_JobRole = role.PK;
			campaign.HV_CampaignStartDate = new ZDate(2008, 2, 1);
			campaign.HV_CampaignEndDate = new ZDate(2008, 3, 1);
			campaign.HV_NumberOfPositionsAvailable = 3;
			campaign.HV_NumberOfPositionsFilled = 1;
			campaign.HV_WageLow = 10000;
			campaign.HV_WageHigh = 10000;
			campaign.HV_RX_NKWageRangeCurrency = "AUD";

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "HRE";
			staff.GS_FullName = "HR Manager";
			campaign.HV_GS_NKControlledBy = "HRE";
		}
	}
}
