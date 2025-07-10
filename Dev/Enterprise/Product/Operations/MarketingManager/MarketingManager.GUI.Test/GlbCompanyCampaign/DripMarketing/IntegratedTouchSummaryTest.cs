using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class IntegratedTouchSummaryTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestViewModel()
		{
			using (var control = new IntegratedTouchSummary())
			{
				AssertNotNull(control.ViewModel);
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); // we are testing the control, not the form
		}

		protected override Form GetFormToBashCore()
		{
			var result = new ZEmptyFormForBasherTest
			{
				CaptionRenderingEnabled = true,
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 1000, true),
			};
			var userControl = new IntegratedTouchSummaryForTest
			{
				Dock = DockStyle.Fill
			};
			result.Controls.Add(userControl);

#if WINZOR
			var campaignTestHelper = new Business.Testing.GlbCompanyCampaignTestHelper(Factory);
			campaignTestHelper.SetupDripCampaign();
			userControl.SetDataContext(campaignTestHelper.Master);
#endif

			return result;
		}

		public void TestLinkControlType()
		{
			using (var control = new IntegratedTouchSummaryForTest())
			{
				Assert(control.ClickStatControl_Exposed is CampaignClickStatDetailsUserControl);
			}
		}

		public void TestDisplayGroupBoxCaptions_SelectTouchCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1A = campaign.AllTouches.AddNew();
			touch1A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch1A.G0_CampaignName = "Touch 1A";
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";

			var touch1B = campaign.AllTouches.AddNew();
			touch1B.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch1B.G0_CampaignName = "Touch 1B";
			touch1B.G0_HorizontalId = 1;
			touch1B.G0_VerticalId = "B";

			var touch2A = campaign.AllTouches.AddNew();
			touch2A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch2A.G0_CampaignName = "Touch 2A";
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";

			using (var integratedTouchSummary = new IntegratedTouchSummaryForTest())
			{
				integratedTouchSummary.SetDataContext(campaign);

				AssertEquals("Campaign Summary (1A)", integratedTouchSummary.TrackingStatusGroupBox_Exposed.CaptionResourceString.Caption);
				AssertEquals("Linked Opportunities", integratedTouchSummary.OpportunitiesGroupBox_Exposed.CaptionResourceString.Caption);

				integratedTouchSummary.SelectCampaign(touch1A);

				AssertEquals("Campaign Summary (1A)", integratedTouchSummary.TrackingStatusGroupBox_Exposed.CaptionResourceString.Caption);
				AssertEquals("Linked Opportunities (1A)", integratedTouchSummary.OpportunitiesGroupBox_Exposed.CaptionResourceString.Caption);

				integratedTouchSummary.SelectCampaign(touch1B);

				AssertEquals("Campaign Summary (1B)", integratedTouchSummary.TrackingStatusGroupBox_Exposed.CaptionResourceString.Caption);
				AssertEquals("Linked Opportunities (1B)", integratedTouchSummary.OpportunitiesGroupBox_Exposed.CaptionResourceString.Caption);

				integratedTouchSummary.SelectCampaign(touch2A);

				AssertEquals("Campaign Summary (2A)", integratedTouchSummary.TrackingStatusGroupBox_Exposed.CaptionResourceString.Caption);
				AssertEquals("Linked Opportunities (2A)", integratedTouchSummary.OpportunitiesGroupBox_Exposed.CaptionResourceString.Caption);
			}
		}

		public void TestRefreshOpportunityCreationChart_SelectTouchCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1A = campaign.AllTouches.AddNew();
			touch1A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch1A.G0_CampaignName = "Touch 1A";

			var touch2A = campaign.AllTouches.AddNew();
			touch2A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch2A.G0_CampaignName = "Touch 2A";

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_Status = "WON";
			opp1.P8_G0 = touch1A.PK;
			Factory.Save();

			using (var integratedTouchSummary = new IntegratedTouchSummaryForTest())
			{
				integratedTouchSummary.SetDataContext(campaign);
				var opportunityCreationControl = integratedTouchSummary.OpportunityCreationControl_Exposed;

				AssertEquals("0", opportunityCreationControl.CurrentOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationControl.WonOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.LostOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.OtherOpportunitiesTotal.Text);
				AssertEquals("100%", opportunityCreationControl.WinRatioOpportunitiesTotal.Text);

				var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp2.P8_Status = "WON";
				opp2.P8_G0 = touch2A.PK;

				var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp3.P8_Status = "CRT";
				opp3.P8_G0 = touch1A.PK;

				var opp4 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp4.P8_Status = "LOS";
				opp4.P8_G0 = touch2A.PK;
				Factory.Save();

				integratedTouchSummary.SelectCampaign(touch1A);
				opportunityCreationControl = integratedTouchSummary.OpportunityCreationControl_Exposed;

				AssertEquals("1", opportunityCreationControl.CurrentOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationControl.WonOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.LostOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.OtherOpportunitiesTotal.Text);
				AssertEquals("50%", opportunityCreationControl.WinRatioOpportunitiesTotal.Text);

				integratedTouchSummary.SelectCampaign(touch2A);
				opportunityCreationControl = integratedTouchSummary.OpportunityCreationControl_Exposed;

				AssertEquals("0", opportunityCreationControl.CurrentOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationControl.WonOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationControl.LostOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.OtherOpportunitiesTotal.Text);
				AssertEquals("50%", opportunityCreationControl.WinRatioOpportunitiesTotal.Text);

				integratedTouchSummary.SelectCampaign(campaign);
				opportunityCreationControl = integratedTouchSummary.OpportunityCreationControl_Exposed;

				AssertEquals("1", opportunityCreationControl.CurrentOpportunitiesTotal.Text);
				AssertEquals("2", opportunityCreationControl.WonOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationControl.LostOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.OtherOpportunitiesTotal.Text);
				AssertEquals("50%", opportunityCreationControl.WinRatioOpportunitiesTotal.Text);
			}
		}

		public void TestRefreshOpportunityCreationChart_RefreshSummary()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1A = campaign.AllTouches.AddNew();
			touch1A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch1A.G0_CampaignName = "Touch 1A";

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_Status = "WON";
			opp1.P8_G0 = touch1A.PK;
			Factory.Save();

			using (var integratedTouchSummary = new IntegratedTouchSummaryForTest())
			{
				integratedTouchSummary.SetDataContext(campaign);
				var opportunityCreationControl = integratedTouchSummary.OpportunityCreationControl_Exposed;

				AssertEquals("0", opportunityCreationControl.CurrentOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationControl.WonOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.LostOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.OtherOpportunitiesTotal.Text);
				AssertEquals("100%", opportunityCreationControl.WinRatioOpportunitiesTotal.Text);

				var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp2.P8_Status = "WON";
				opp2.P8_G0 = touch1A.PK;

				var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp3.P8_Status = "CRT";
				opp3.P8_G0 = touch1A.PK;

				var opp4 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp4.P8_Status = "LOS";
				opp4.P8_G0 = touch1A.PK;
				Factory.Save();

				integratedTouchSummary.RefreshSummary(touch1A);
				opportunityCreationControl = integratedTouchSummary.OpportunityCreationControl_Exposed;

				AssertEquals("1", opportunityCreationControl.CurrentOpportunitiesTotal.Text);
				AssertEquals("2", opportunityCreationControl.WonOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationControl.LostOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationControl.OtherOpportunitiesTotal.Text);
				AssertEquals("50%", opportunityCreationControl.WinRatioOpportunitiesTotal.Text);
			}
		}

		public void TestTransitionProgressGroupBoxVisibility()
		{
			using var integratedTouchSummary = new IntegratedTouchSummaryForTest();
			var groupBox = integratedTouchSummary.Controls.Find("TransitionProgressGroupBox", searchAllChildren: true).Single();
#if WINZOR
			Assert("TransitionProgressGroupBox should be hidden for Winzor", !groupBox.Visible);
#else
			Assert("TransitionProgressGroupBox should be visible by default", groupBox.Visible);
#endif
		}

		class IntegratedTouchSummaryForTest : IntegratedTouchSummary
		{
			public UserControl ClickStatControl_Exposed => ClickStatControl;
			public OpportunityCreationChartUserControl OpportunityCreationControl_Exposed => OpportunityCreationControl;
			public ZGroupBox TrackingStatusGroupBox_Exposed => TrackingStatusGroupBox;
			public ZGroupBox OpportunitiesGroupBox_Exposed => OpportunitiesGroupBox;

			public void SelectCampaign(GlbCompanyCampaign campaign)
			{
				OnCampaignSelected(null, new CampaignSelectedEventArgs(campaign));
			}

			public void RefreshSummary(GlbCompanyCampaign campaign)
			{
				ClickStatControl.SetDataBinding(campaign.StatModel, "");
				OnControlOnEndLongRefresh(null, new EventArgs());
			}
		}
	}
}
