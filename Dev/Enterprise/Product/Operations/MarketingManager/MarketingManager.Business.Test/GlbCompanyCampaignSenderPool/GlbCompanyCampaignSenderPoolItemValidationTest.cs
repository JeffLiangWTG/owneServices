using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSenderPoolItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckGCP_GS_NKSender()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			var poolItem = Factory.New<GlbCompanyCampaignSenderPoolItem>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			glbStaff.GS_GB_HomeBranch = branch.PK;
			poolItem.GCP_G0_Campaign = campaign.PK;

			poolItem.GCP_GS_NKSender = ZString.Empty;
			AssertHasErrors("Campaign Sender is mandatory", poolItem.GCP_GS_NKSenderInfo);

			poolItem.GCP_GS_NKSender = glbStaff.GS_Code;
			AssertHasErrors("Staff without email address", poolItem.GCP_GS_NKSenderInfo);

			glbStaff.GS_EmailAddress = "e";
			poolItem.GCP_GS_NKSender = glbStaff.GS_Code;
			AssertHasErrors("Staff without valid email address", poolItem.GCP_GS_NKSenderInfo);

			glbStaff.GS_EmailAddress = "e@ma.il";
			poolItem.GCP_GS_NKSender = glbStaff.GS_Code;
			AssertNoErrors("Staff with valid email", poolItem.GCP_GS_NKSenderInfo);

			glbStaff.GS_GB_HomeBranch = ZGuid.Empty;
			poolItem.GCP_GS_NKSender = glbStaff.GS_Code;
			AssertHasErrors("Staff without valid a branch", poolItem.GCP_GS_NKSenderInfo);

			Factory.Save();

			string gsCode = glbStaff.GS_Code;
			glbStaff.Delete();
			Factory.Save();

			poolItem.GCP_GS_NKSender = gsCode;
			AssertHasErrors("Manager has been removed, invalid staff code", poolItem.GCP_GS_NKSenderInfo);
		}

		public void TestCheckGCP_SendRatio()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_GB_HomeBranch = Env.CurrentBranchPK;
			var poolItem = Factory.New<GlbCompanyCampaignSenderPoolItem>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			poolItem.GCP_G0_Campaign = campaign.PK;
			poolItem.GCP_GS_NKSender = glbStaff.GS_Code;

			poolItem.Validation.ValidateAll();
			AssertNoErrors("Should not have errors.", poolItem);

			poolItem.GCP_SendRatio = -1;
			AssertHasErrors("Should have an error.", poolItem.GCP_SendRatioInfo);

			poolItem.GCP_SendRatio = 0;
			AssertHasErrors("Should have an error.", poolItem.GCP_SendRatioInfo);

			poolItem.GCP_SendRatio = 1;
			AssertNoErrors("Should not have errors.", poolItem);
			AssertNoErrors("Should have an error.", poolItem.GCP_SendRatioInfo);
		}
	}
}
