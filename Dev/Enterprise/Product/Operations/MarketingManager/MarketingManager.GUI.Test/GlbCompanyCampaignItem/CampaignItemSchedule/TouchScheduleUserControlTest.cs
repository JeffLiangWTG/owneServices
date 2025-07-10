using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TouchScheduleUserControlTest : TestCaseWithFactory
	{
		public void TestBatchControlVisibility_OpportunityCreation()
		{
			AssertVisibilityBatchControl(InsideSalesTouchTypeList.Codes.OpportunityCreation, true);
		}

		public void TestBatchControlVisibility_PreApproachEmail()
		{
			AssertVisibilityBatchControl(InsideSalesTouchTypeList.Codes.PreApproachEmail, true);
		}

		public void TestBatchControlVisibility_Broadcast()
		{
			AssertVisibilityBatchControl(CampaignTypeList.Codes.Broadcast, true);
		}

		public void TestBatchControlVisibility_LinkTracking()
		{
			AssertVisibilityBatchControl(CampaignTypeList.Codes.LinkTracking, true);
		}

		public void TestBatchControlVisibility_Survey()
		{
			AssertVisibilityBatchControl(CampaignTypeList.Codes.Survey, true);
		}

		public void TestBatchControlVisibility_TargetList()
		{
			AssertVisibilityBatchControl(CampaignTypeList.Codes.TargetList, true);
		}

		public void TestBatchControlVisibility_Voting()
		{
			AssertVisibilityBatchControl(CampaignTypeList.Codes.Voting, true);
		}

		void AssertVisibilityBatchControl(string touchType, bool isVisible, bool isEmptyCampaign = false)
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = touchType;

			var settings = Factory.NewWithValidTestData<GlbCompanyCampaignSendSettings>();
			settings.GSC_G0_Campaign = isEmptyCampaign ? ZGuid.Empty : campaign.PK;

			Factory.Save();

			using (var form = new ZForm(settings))
			using (var touchSetupScheduleUserControl = new TouchScheduleUserControlForTest())
			{
				form.Controls.Add(touchSetupScheduleUserControl);
				form.Show();

				AssertEquals(isVisible, touchSetupScheduleUserControl.GroupBoxOptionsExposed.Visible);
				AssertEquals(isVisible, touchSetupScheduleUserControl.PanelScheduleDateTimeExposed.Visible);
				AssertEquals(isVisible, touchSetupScheduleUserControl.PanelZonesExposed.Visible);
				AssertEquals(isVisible, touchSetupScheduleUserControl.GroupBoxContactLimitPerOrganizationExposed.Visible);
				AssertEquals(isVisible, touchSetupScheduleUserControl.RadioBatchRecurrencePatternExposed.Visible);
				AssertEquals(isVisible, touchSetupScheduleUserControl.PanelBatchControlRulesExposed.Visible);
				AssertEquals(isVisible, touchSetupScheduleUserControl.RecurrenceControlExposed.Visible);
			}
		}

		public void TestBatchControlVisibilityWithoutOwnerCampaign()
		{
			AssertVisibilityBatchControl(InsideSalesTouchTypeList.Codes.OpportunityCreation, true, true);
			AssertVisibilityBatchControl(InsideSalesTouchTypeList.Codes.PreApproachEmail, true, true);
			AssertVisibilityBatchControl(CampaignTypeList.Codes.Broadcast, true, true);
			AssertVisibilityBatchControl(CampaignTypeList.Codes.LinkTracking, true, true);
			AssertVisibilityBatchControl(CampaignTypeList.Codes.Survey, true, true);
			AssertVisibilityBatchControl(CampaignTypeList.Codes.TargetList, true, true);
			AssertVisibilityBatchControl(CampaignTypeList.Codes.Voting, true, true);
		}
	}
}
