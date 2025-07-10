using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class TouchSummaryGridTest : ZFormBasherTest
	{
		public void TestColumns()
		{
			using var form = GetFormToBash();
			form.Show();

			var touchGrid = (ZGrid)form.Controls.Find("touchGrid", searchAllChildren: true).Single();
			var expectedColumns = new[]
			{
				"TouchId",
				"G0_CampaignNameMultilingual",
				"SummaryStats+TotalCount",
				"SummaryStats+TouchSummaryVerifiedCount",
				"SummaryStats+TouchSummaryUnverifiedCount",
				"SummaryStats+NonDeliveredCount",
				"SummaryStats+QueuedCount",
				"SummaryStats+ScheduledCount",
				"SummaryStats+UnScheduledCount",
				"SummaryStats+SentCount",
				"SummaryStats+UnsubscribedCount",
				"SummaryStats+FailedToTransitionCount",
				"SummaryStats+TransitionedToNextTouchCount"
			};

			var actualColumns = touchGrid.Columns.Select(c => c.ColumnName);
			AssertContainsExactElementsInAnyOrder(expectedColumns, actualColumns);
		}

		public void TestAddHorizontalButton()
		{
			using var form = GetFormToBash();
			form.Show();

			ClickToolStripButton(form, "addHorizontalButton");
			AssertLastOpenedCampaign(form, 3, "A");
		}

		public void TestAddVerticalButton()
		{
			using var form = GetFormToBash();
			form.Show();
			var touchGrid = (ZGrid)form.Controls.Find("touchGrid", searchAllChildren: true).Single();

			touchGrid.ListManager.Position = 2;
			ClickToolStripButton(form, "addVerticalButton");
			AssertLastOpenedCampaign(form, 2, "B");
		}

		public void TestRemoveCampaignButton()
		{
			using var form = GetFormToBash();
			form.Show();
			var touchGrid = (ZGrid)form.Controls.Find("touchGrid", searchAllChildren: true).Single();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			touchGrid.ListManager.Position = 1;
			ClickToolStripButton(form, "removeCampaignButton");

			AssertEquals("Are you sure you want to delete Touch [1B] - Touch 1B", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Touch should not be deleted when user selects No", 3, campaign.AllTouches.Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			ClickToolStripButton(form, "removeCampaignButton");

			AssertEquals("Are you sure you want to delete Touch [1B] - Touch 1B", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Touch should be deleted when user selects Yes", 2, campaign.AllTouches.Count);
		}

		public void TestRefreshButton()
		{
			using var form = GetFormToBash();
			form.Show();
			var userControl = (TouchSummaryGrid)form.Controls.Find("TouchSummaryGrid", searchAllChildren: false).Single();

			var refreshEvents = new List<Tuple<string, string>>();
			userControl.BeginLongRefresh += (sender, e) => refreshEvents.Add(Tuple.Create("BeginLongRefresh", e.StatusMessage));
			userControl.LongRefreshProgress += (sender, e) => refreshEvents.Add(Tuple.Create("LongRefreshProgress", e.StatusMessage));
			userControl.EndLongRefresh += (sender, e) => refreshEvents.Add(Tuple.Create("EndLongRefresh", string.Empty));

			ClickToolStripButton(form, "refreshButton");

			var expectedRefreshEvents = new[]
			{
				Tuple.Create("BeginLongRefresh", "Refreshing summary data..."),
				Tuple.Create("LongRefreshProgress", "Loading Summary Stats..."),
				Tuple.Create("EndLongRefresh", string.Empty),
				Tuple.Create("LongRefreshProgress", "Refreshing Summary Stats..."),
				Tuple.Create("LongRefreshProgress", "Refreshing Summary Stats..."),
				Tuple.Create("LongRefreshProgress", "Refreshing Summary Stats..."),
				Tuple.Create("EndLongRefresh", string.Empty),
				Tuple.Create("EndLongRefresh", string.Empty)
			};
			AssertContainsExactElementsInExactOrder(expectedRefreshEvents, refreshEvents);
		}

		public void TestCampaignSelected_OnPositionChanged()
		{
			using var form = GetFormToBash();
			form.Show();
			var userControl = (TouchSummaryGrid)form.Controls.Find("TouchSummaryGrid", searchAllChildren: false).Single();
			var touchGrid = (ZGrid)userControl.Controls.Find("touchGrid", searchAllChildren: true).Single();

			GlbCompanyCampaign selectedCampaign = null;
			userControl.CampaignSelected += (sender, e) => selectedCampaign = e.GlbCompanyCampaign;

			touchGrid.ListManager.Position = 2;
			Assert("CampaignSelected event should have been raised", selectedCampaign != null);
			AssertEquals("Touch 2A", selectedCampaign.G0_CampaignName);
		}

		void AssertLastOpenedCampaign(Form form, ZByte horizontalId, ZString verticalId)
		{
			var userControl = (TouchSummaryGrid)form.Controls.Find("TouchSummaryGrid", searchAllChildren: false).Single();
			var lastShownForm = userControl.ViewModel.LastController.LastShownForm as GlbCompanyCampaignForm;
			var newTouch = lastShownForm.BusinessEntity as GlbCompanyCampaign;

			AssertEquals("Horizontal ID should be set for new touch campaign", horizontalId, newTouch.G0_HorizontalId);
			AssertEquals("Vertical ID should be set for new touch campaign", verticalId, newTouch.G0_VerticalId);

			lastShownForm.Close();
			lastShownForm.Dispose();
		}

		void ClickToolStripButton(Form form, string buttonName)
		{
			var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", searchAllChildren: true).Single();
			var toolStripButton = toolStrip.Items[buttonName];
			toolStripButton.PerformClick();
		}

		protected override Form GetFormToBashCore()
		{
			var result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Size = ControlDpiScalingHelper.NewScaledSize(1000, 500, isInStandardDpi: true);

			var userControl = new TouchSummaryGrid();
			userControl.Dock = DockStyle.Fill;
			userControl.SetDataContext(campaign);
			result.Controls.Add(userControl);

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1A = campaign.AllTouches.AddNew();
			touch1A.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.Broadcast;
			touch1A.G0_CampaignName = "Touch 1A";
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";

			var touch1B = campaign.AllTouches.AddNew();
			touch1B.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.Survey;
			touch1B.G0_CampaignName = "Touch 1B";
			touch1B.G0_HorizontalId = 1;
			touch1B.G0_VerticalId = "B";

			var touch2A = campaign.AllTouches.AddNew();
			touch2A.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.Voting;
			touch2A.G0_CampaignName = "Touch 2A";
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			Factory.Save();
		}

		GlbCompanyCampaign campaign;
	}
}
