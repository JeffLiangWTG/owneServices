using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignDripMarketingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckGCD_ParentHorizontalId()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var campaign = master.AllTouches.AddNew();

			var pivot = Factory.New<GlbCompanyCampaignDripMarketing>();

			pivot.Validation.ValidateAll();
			AssertEquals(false, pivot.GCD_ParentHorizontalIdInfo.HasErrors());

			pivot.GCD_ParentHorizontalId = 1;
			AssertEquals(false, pivot.GCD_ParentHorizontalIdInfo.HasErrors());

			pivot.GCD_ParentHorizontalId = 0;
			pivot.GCD_G0_ParentTouch = campaign.PK;
			pivot.Validation.ValidateAll();
			AssertEquals(true, pivot.GCD_ParentHorizontalIdInfo.HasErrors());

			pivot.GCD_ParentHorizontalId = 1;
			AssertEquals(false, pivot.GCD_ParentHorizontalIdInfo.HasErrors());

			pivot.GCD_ParentHorizontalId = 0;
			pivot.GCD_G0_ParentTouch = master.PK;
			pivot.Validation.ValidateAll();
			AssertEquals(false, pivot.GCD_ParentHorizontalIdInfo.HasErrors());
		}

		public void TestCheckGCD_G0_ParentTouch()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = master.AllTouches.AddNew();
			var pivot = Factory.New<GlbCompanyCampaignDripMarketing>();

			pivot.Validation.ValidateAll();
			Assert(pivot.GCD_G0_ParentTouchInfo.HasErrors());

			pivot.GCD_G0_ParentTouch = touch.PK;
			Assert(!pivot.GCD_G0_ParentTouchInfo.HasErrors());

			pivot.GCD_G0_ParentTouch = ZGuid.Empty;
			Assert(pivot.GCD_G0_ParentTouchInfo.HasErrors());
		}
	}
}
