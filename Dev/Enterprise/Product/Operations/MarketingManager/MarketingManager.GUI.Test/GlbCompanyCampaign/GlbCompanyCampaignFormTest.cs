using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OxyPlot;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI
{
	[TestedType(typeof(GlbCompanyCampaignForm))]
	sealed class GlbCompanyCampaignFormTest : ZFormBasherTest
	{
		#region Touches

		public void TestNewDripMarketingPopup()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(master, master.Factory, true);

			using (var form = GetNewGlbCompanyCampaignForm(master))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				form.FireSaveButton();

				AssertEquals("Do you want to create your first Touch now?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNewDripMarketingPopup_AfterFailedSave()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (var form = GetNewGlbCompanyCampaignForm(master))
			{
				master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				form.FireSaveButton();

				GlbCompanyCampaignTestHelper.PopulateCampaign(master, master.Factory, true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.FireSaveButton();

				const string question = "Do you want to create your first Touch now?";
				AssertEquals(question, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, UnitTestUserNotification.Instance.PreviousMessages.Count(c => c.Text == question));
			}
		}

		public void TestNewDripMarketingPopup_NotDRM()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(master, master.Factory, true);

			using (var form = GetNewGlbCompanyCampaignForm(master))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				form.FireSaveButton();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDripTabsVisible()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals(false, form.TouchesTabPage.TabVisible);
				AssertEquals(false, form.TouchSetupTabPage.TabVisible);
				AssertEquals(true, form.UseLastEmailSenderAddressCheckBox.Visible);
			}

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals(true, form.TouchesTabPage.TabVisible);
				AssertEquals(false, form.TouchSetupTabPage.TabVisible);
				AssertEquals(false, form.UseLastEmailSenderAddressCheckBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				AssertEquals(false, form.TouchesTabPage.TabVisible);
				AssertEquals(false, form.TouchSetupTabPage.TabVisible);
				AssertEquals(true, form.UseLastEmailSenderAddressCheckBox.Visible);
			}

			campaign.G0_G0_Master = master.PK;
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals(false, form.TouchesTabPage.TabVisible);
				AssertEquals(true, form.TouchSetupTabPage.TabVisible);
				AssertEquals(true, form.UseLastEmailSenderAddressCheckBox.Visible);
			}
		}

		public void TestMasterShowHideCampaignBatchCountControls()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				Assert(form.CampaignDocumentGroupBox.Visible);
				Assert(form.BatchCountCalcEdit.Visible);
				Assert(form.OrLabel.Visible);
				Assert(form.FilterForToAllContactsCheckbox.Visible);
				Assert(form.UseCampaignCheckBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				Assert(!form.CampaignDocumentGroupBox.Visible);
				Assert(form.BatchCountCalcEdit.Visible);
				Assert(form.OrLabel.Visible);
				Assert(form.FilterForToAllContactsCheckbox.Visible);
				Assert(!form.UseCampaignCheckBox.Visible);
			}
		}

		public void TestVisibilitySendingOptionsGroupBox_MasterCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
				Assert(form.SendingOptionsGroupBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				Assert(form.SendingOptionsGroupBox.Visible);
			}
		}

		public void TestVisibilitySendingOptionsGroupBox_StandAloneCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				Assert(form.SendingOptionsGroupBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				Assert(form.SendingOptionsGroupBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
				Assert(form.SendingOptionsGroupBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				Assert(form.SendingOptionsGroupBox.Visible);
			}
		}

		public void TestVisibilitySendingOptionsGroupBox_DRMCampaign()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touchCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touchCampaign.G0_G0_Master = masterCampaign.PK;

			using (var form = GetNewGlbCompanyCampaignForm(touchCampaign))
			{
				form.Show();

				touchCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				Assert(!form.SendingOptionsGroupBox.Visible);

				touchCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				Assert(!form.SendingOptionsGroupBox.Visible);

				touchCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
				Assert(!form.SendingOptionsGroupBox.Visible);

				touchCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				Assert(!form.SendingOptionsGroupBox.Visible);
			}
		}

		public void TestVisibilitySendingOptionsGroupBox_INSCampaign()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touchCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touchCampaign.G0_G0_Master = masterCampaign.PK;

			using (var form = GetNewGlbCompanyCampaignForm(touchCampaign))
			{
				form.Show();

				touchCampaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
				Assert(!form.SendingOptionsGroupBox.Visible);

				touchCampaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
				Assert(!form.SendingOptionsGroupBox.Visible);
			}
		}

		public void TestCaptionSendingOptionsGroupBox()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				AssertEquals("Contact Filter Batch Options", form.SendingOptionsGroupBox.CaptionResourceString.Caption);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				AssertEquals("Batch Options", form.SendingOptionsGroupBox.CaptionResourceString.Caption);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				AssertEquals("Batch Options", form.SendingOptionsGroupBox.CaptionResourceString.Caption);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
				AssertEquals("Batch Options", form.SendingOptionsGroupBox.CaptionResourceString.Caption);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				AssertEquals("Batch Options", form.SendingOptionsGroupBox.CaptionResourceString.Caption);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
				AssertEquals("Contact Filter Batch Options", form.SendingOptionsGroupBox.CaptionResourceString.Caption);

				campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
				AssertEquals("Batch Options", form.SendingOptionsGroupBox.CaptionResourceString.Caption);

				campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
				AssertEquals("Batch Options", form.SendingOptionsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestSendingTabVisible()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals(true, form.SendingTabPage.TabVisible);
			}

			campaign.G0_G0_Master = master.PK;
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals(false, form.SendingTabPage.TabVisible);
			}

			using (var form = GetNewGlbCompanyCampaignForm(master))
			{
				form.Show();
				AssertEquals(true, form.SendingTabPage.TabVisible);
			}
		}

		#endregion

		public void TestActionCallback()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Name";

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.SelectEmailDesignerTab(ActionCallback);
				AssertEquals(false, ActionCalled);

				form.sendEmailToSelectedButton.PerformClick();
				AssertEquals(true, ActionCalled);
			}
		}

		public void TestVotingTabPosition()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign";

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			using (var voteCampaignPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.VoteCampaignPlugIn))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				AssertEquals(true, voteCampaignPlugIn.Enabled);
				AssertEquals("PlugIn should be the third tab page", form.GetTabIndexForSurveyAndVote(), form.TopLevelTabControl_Exposed.TabPages.IndexOf(voteCampaignPlugIn.TabPage));

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				AssertEquals(false, voteCampaignPlugIn.Enabled);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				AssertEquals(true, voteCampaignPlugIn.Enabled);
				AssertEquals("PlugIn should be the third tab page", form.GetTabIndexForSurveyAndVote(), form.TopLevelTabControl_Exposed.TabPages.IndexOf(voteCampaignPlugIn.TabPage));
			}
		}

		public void TestSurveyTabPosition()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign";

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			using (var surveyCampaignPlugin = form.PlugIns.GetPlugIn(ControllerIDs.SurveyCampaignPlugIn))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				AssertEquals(true, surveyCampaignPlugin.Enabled);
				AssertEquals("PlugIn should be the third tab page", form.GetTabIndexForSurveyAndVote(), form.TopLevelTabControl_Exposed.TabPages.IndexOf(surveyCampaignPlugin.TabPage));

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
				AssertEquals(false, surveyCampaignPlugin.Enabled);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				AssertEquals(true, surveyCampaignPlugin.Enabled);
				AssertEquals("PlugIn should be the third tab page", form.GetTabIndexForSurveyAndVote(), form.TopLevelTabControl_Exposed.TabPages.IndexOf(surveyCampaignPlugin.TabPage));
			}
		}

		public void TestVotingAndSurveyTabPositionWhenChangingCampaignType()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign";

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			using (var voteCampaignPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.VoteCampaignPlugIn))
			using (var surveyCampaignPlugin = form.PlugIns.GetPlugIn(ControllerIDs.SurveyCampaignPlugIn))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				AssertEquals(false, voteCampaignPlugIn.Enabled);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				AssertEquals(true, surveyCampaignPlugin.Enabled);
				AssertEquals("PlugIn should be the third tab page", form.GetTabIndexForSurveyAndVote(), form.TopLevelTabControl_Exposed.TabPages.IndexOf(surveyCampaignPlugin.TabPage));

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				AssertEquals(false, voteCampaignPlugIn.Enabled);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				AssertEquals(true, voteCampaignPlugIn.Enabled);
				AssertEquals(false, surveyCampaignPlugin.Enabled);
				AssertEquals("PlugIn should be the third tab page", form.GetTabIndexForSurveyAndVote(), form.TopLevelTabControl_Exposed.TabPages.IndexOf(voteCampaignPlugIn.TabPage));

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				AssertEquals(true, surveyCampaignPlugin.Enabled);
				AssertEquals(false, voteCampaignPlugIn.Enabled);
				AssertEquals("PlugIn should be the third tab page", form.GetTabIndexForSurveyAndVote(), form.TopLevelTabControl_Exposed.TabPages.IndexOf(surveyCampaignPlugin.TabPage));

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
				AssertEquals(false, voteCampaignPlugIn.Enabled);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				AssertEquals(true, voteCampaignPlugIn.Enabled);
				AssertEquals("PlugIn should be the third tab page", form.GetTabIndexForSurveyAndVote(), form.TopLevelTabControl_Exposed.TabPages.IndexOf(voteCampaignPlugIn.TabPage));
			}
		}

		public void TestSendScheduleTabPageSelected_AllItemsDeleted()
		{
			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_Code = "COR";
			coordinator.GS_EmailAddress = "unit.test@cw1.com";

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			manager.GS_Code = "MGR";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_CampaignName = "Test Campaign";
			campaign.G0_Category = "PRINT";
			campaign.G0_Type = "PREAP";
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			campaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaign.G0_GS_NKCampaignManager = manager.GS_Code;

			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				campaign.CampaignItemSchedule.ItemsDeleted = ZBool.True;

				form.MainTabControl_Exposed.SelectedTab = form.SendScheduleTabPage;

				AssertNull("Should be no Last message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddSurveyQuestionsAndChangeCampaignType()
		{
			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_Code = "COR";
			coordinator.GS_EmailAddress = "unit.test@cw1.com";

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			manager.GS_Code = "MGR";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_CampaignName = "Test Campaign";
			campaign.G0_Category = "PRINT";
			campaign.G0_Type = "PREAP";
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			campaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaign.G0_GS_NKCampaignManager = manager.GS_Code;

			var question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			question.HY_Question = "Question 1";
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			using (var surveyCampaignPlugin = form.PlugIns.GetPlugIn(ControllerIDs.SurveyCampaignPlugIn))
			using (var surveyCampaignUserControl = (VoteExamSurveyUserControl)surveyCampaignPlugin.UserControl)
			{
				form.Show();
				form.MainTabControl_Exposed.SelectedTab = surveyCampaignPlugin.TabPage;

				var question2 = campaign.Questions.AddNew();
				question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
				question2.HY_Question = "Question 2";
				form.FireSaveButton();
				Application.DoEvents();

				form.MainTabControl_Exposed.SelectedTab = form.MainTabPage_Exposed;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertExceptionThrown<CannotDeleteException>(() => campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast);
				form.FireSaveButton();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertExceptionThrown<CannotDeleteException>(() => campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey);

				form.MainTabControl_Exposed.SelectedTab = surveyCampaignPlugin.TabPage;

				surveyCampaignUserControl.QuestionsControl.QuestionsGrid.PerformMouseDownForTest(0, 1);
				Application.DoEvents();
				AssertEquals(surveyCampaignUserControl.QuestionDetailsControl.zTextBox1.Text, question.HY_Question);

				surveyCampaignUserControl.QuestionsControl.QuestionsGrid.PerformMouseDownForTest(1, 1);
				Application.DoEvents();
				AssertEquals(surveyCampaignUserControl.QuestionDetailsControl.zTextBox1.Text, question2.HY_Question);
			}
		}

		public void TestNoConcurrencyErrorWhenAccessingEDocs()
		{
			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_Code = "COR";
			coordinator.GS_EmailAddress = "unit.test@cw1.com";

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			manager.GS_Code = "MGR";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Name";
			campaign.G0_Category = "PRINT";
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			campaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaign.G0_GS_NKCampaignManager = manager.GS_Code;
			campaign.G0_Type = "PREAP";
			campaign.G0_BroadcastVoteSurveyExam = "BRD";
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				var plugin = (DocumentScanning.PlugIn.eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				plugin.SelectTabPage();

				campaign.G0_CampaignName = "Test Name 2";
				campaign.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs.Load<GlbCompanyCampaign>(campaign.PK).MarkLightValidationAsValidForTesting();

				AssertNoExceptionThrown(() => form.ValidateAndSave_Exposed());
				AssertEquals("Campaign name should have successfully updated.", "Test Name 2", campaign.G0_CampaignName);
			}
		}
		public void TestActionCallback_WhenTabChanges()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Name";

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.SelectEmailDesignerTab(ActionCallback);
				AssertEquals(false, ActionCalled);

				form.SelectTab(form.TrackingTabPage);
				AssertEquals(false, ActionCalled);
			}
		}

		bool ActionCalled;

		void ActionCallback()
		{
			ActionCalled = true;
		}

		public void TestUseCampaignName()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Name";

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				Assert("checked by default", form.UseCampaignCheckBox.Checked);
				Assert("readonly by default", form.EmailSubjectTextBox.ReadOnly);
				AssertEquals("should take bizO campaign name", "Test Name", form.CampaignNameTextBox.Text);
				AssertEquals("should take campaign name", "Test Name", form.EmailSubjectTextBox.Text);

				form.UseCampaignCheckBox.Checked = false;
				Assert(!form.UseCampaignCheckBox.Checked);
				Assert("textbox should be free", !form.EmailSubjectTextBox.ReadOnly);

				form.EmailSubjectTextBox.Text = "a different name"; //change to a different name

				form.UseCampaignCheckBox.Checked = true;
				Assert(form.UseCampaignCheckBox.Checked);
				Assert("should be readonly ", form.EmailSubjectTextBox.ReadOnly);
				AssertEquals("should take campaign name", "Test Name", form.EmailSubjectTextBox.Text);
			}
		}

		public void TestG0_StoreEmailInEDocs_SaveFalseWhenIsNotBRD()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Name";
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				Assert("Pre-Condition", !form.PermanentlySaveAgainstRecipientCheckBox.Checked);
				Assert("Pre-Condition", !campaign.G0_StoreEmailInEDocs);

				form.PermanentlySaveAgainstRecipientCheckBox.Checked = true;

				Assert("G0_StoreEmailInEDocs should be true", campaign.G0_StoreEmailInEDocs);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

				Assert("non-BRD G0_StoreEmailInEDocs should be false", !campaign.G0_StoreEmailInEDocs);
			}
		}

		public void TestPermanentlySaveAgainstRecipientCheckBox_OnlyVisibleInBRD()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Name";
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				Assert("PermanentlySaveAgainstRecipientCheckBox should be hidden for non-BRD campaigns", !form.PermanentlySaveAgainstRecipientCheckBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

				Assert("PermanentlySaveAgainstRecipientCheckBox should be hidden for non-BRD campaigns", !form.PermanentlySaveAgainstRecipientCheckBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

				Assert("PermanentlySaveAgainstRecipientCheckBox should be visible for BRD campaigns", form.PermanentlySaveAgainstRecipientCheckBox.Visible);
			}
		}

		#region Hide 'Filter and Send' Tab

		public void TestHideSendCampaignTabPage()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (GlbCompanyCampaignForm form = new GlbCompanyCampaignForm(campaign, true))
			{
				Assert("Should be visible", form.SendingTabPage.TabVisible);
			}

			using (GlbCompanyCampaignForm form = new GlbCompanyCampaignForm(campaign, false))
			{
				Assert("Should be invisible", !form.SendingTabPage.TabVisible);
			}
		}

		#endregion

		#region Attachments

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGlbCompanyCampaignForm_DragDrop()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();

				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MarketingManager\MarketingManager.GUI\Testing\TestTextFile.txt");

				var dataToDrop = new DataObject(DataFormats.FileDrop, new string[] { filePath });
				var args = new DragEventArgs(dataToDrop, 0, 10, 10, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);

				form.OnDragDrop_Exposed(args);
				AssertEquals(1, form.AttachmentsGrid.List.Count);
				AssertEquals("TestTextFile.txt", ((GlbCompanyCampaignAttachmentItem)form.AttachmentsGrid.List[0]).FileName);
			}
		}

		public void TestDocumentAttachmentsRefreshed()
		{
			using (var file1 = TempFile.NewWithExtension("txt"))
			using (var file2 = TempFile.NewWithExtension("txt"))
			{
				var campaign = Factory.New<GlbCompanyCampaign>();
				File.WriteAllBytes(file1.Filename, new byte[] { 1, 2, 3 });
				File.WriteAllBytes(file2.Filename, new byte[] { 1, 2, 3 });

				using (var form = GetNewGlbCompanyCampaignForm(campaign))
				{
					form.Show();
					form.MainTabControl_Exposed.SelectedIndex = 2;

					AssertEquals(0, form.AttachmentsGrid.List.Count);
					IeDoc newFile1 = campaign.DocManagerInfo.AddFileOrDocument(file1.Filename, "MSC");
					newFile1.Description = "temp file 1";
					AssertEquals(0, form.AttachmentsGrid.List.Count);

					form.MainTabControl_Exposed.SelectedIndex = 1;
					AssertEquals(1, form.AttachmentsGrid.List.Count);
				}
			}
		}

		#endregion

		#region Form Caption

		public void TestFormCaption()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignID = "TST00001000";
			campaign.G0_CampaignName = "Test Campaign Name";

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				AssertEquals("Form Caption should be", "Campaign: TST00001000 - Test Campaign Name", form.FormCaption);
			}
		}

		public void TestFormCaption_IsTouchCampaign()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignID = "TST00001000";
			campaign.G0_CampaignName = "Test Campaign";

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				campaign.G0_G0_Master = ZGuid.NewZGuid();
				AssertEquals("Form Caption should be", "Touch: TST00001000 - Test Campaign", form.FormCaption);

				campaign.G0_G0_Master = ZGuid.Empty;
				AssertEquals("Form Caption should be", "Campaign: TST00001000 - Test Campaign", form.FormCaption);
			}
		}

		public void TestControlsCaption_IsTouchCampaign()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			campaign.G0_Category = "Doc";
			campaign.G0_Type = "Jam";
			campaign.G0_G0_Master = ZGuid.NewZGuid();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				AssertEquals("CampaignNameTextBox Caption should be", "Touch Name", form.CampaignNameTextBox.CaptionResourceString.Caption);
				AssertEquals("CampaignTypeDropEdit Caption should be", "Touch Type", form.CampaignTypeDropEdit.CaptionResourceString.Caption);
				AssertEquals("CampaignDocumentGroupBox Caption should be", "Touch Sending Options", form.CampaignDocumentGroupBox.CaptionResourceString.Caption);
			}
		}

		#endregion

		#region Send Campaigns Control

		public void TestSendCampaignsControl()
		{
			var campaign = Factory.New<GlbCompanyCampaignTest.GlbCompanyCampaignForTest>();
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				Assert("Send Campaigns Control should be added to Sending Tabpage", form.SendingTabPage.Controls.Contains(form.SendCampaignControl));
			}
		}

		#endregion

		#region Campaign Document Selection

		public void TestDocumentFieldHelpButton_Click()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.LastFormShownForTest = null;

			var campaign = Factory.New<GlbCompanyCampaign>();

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.DocumentFieldHelpButton_Click(null, EventArgs.Empty);
				AssertEquals("Last shown form should be type of", typeof(MapTreeForm), ZFormModaliser.LastFormShownForTest.GetType());
			}

			if (ZFormModaliser.LastFormShownForTest != null)
			{
				ZFormModaliser.LastFormShownForTest.Dispose();
			}
		}

		#endregion

		#region Form/Control Events

		public void TestLoaded()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_ReplyToEmail = "";

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				Assert("Reply to is checked", form.ReplyToCheckBox.Checked);
				AssertEquals("Reply to textbox is readonly", true, form.ReplyToTextBox.ReadOnly);
			}

			campaign.G0_ReplyToEmail = "xwinter@gmail.com";

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				Assert("Reply to is checked", form.ReplyToCheckBox.Checked);
				AssertEquals("Reply to textbox is readonly", true, form.ReplyToTextBox.ReadOnly);
			}
		}

		public void TestEmailSenderDropEdit_SelectedIndexChanged()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "xwinter@cargowise.com";
			staff.GS_Code = "COR";
			staff.GS_FullName = "Edward Wang";

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(true, form.EmailSendersNameTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressTextBox.Visible);
				AssertEquals(true, form.SenderEmailAddressDropEdit.Visible);
				AssertEquals(false, form.StaffAssignmentDropEdit.Visible);
				AssertEquals(false, form.SenderPoolButton.Visible);

				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
				form.G0_EmailSenderOption_ValueChanged(null, EventArgs.Empty);
				AssertEquals(true, form.EmailSendersNameTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressTextBox.Visible);
				AssertEquals(true, form.SenderEmailAddressDropEdit.Visible);
				AssertEquals("Staff assignment droplist is hidden by default", false, form.StaffAssignmentDropEdit.Visible);
				AssertEquals("Staff Pool is hidden by default", false, form.SenderPoolButton.Visible);
				AssertEquals("", form.EmailSendersNameTextBox.Text);
				AssertEquals("", form.SenderEmailAddressDropEdit.Text);
				AssertEquals("", form.ReplyToTextBox.Text);

				campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
				form.G0_EmailSenderOption_ValueChanged(null, EventArgs.Empty);
				AssertEquals(true, form.EmailSendersNameTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressTextBox.Visible);
				AssertEquals(true, form.SenderEmailAddressDropEdit.Visible);
				AssertEquals(false, form.StaffAssignmentDropEdit.Visible);
				AssertEquals(false, form.SenderPoolButton.Visible);
				AssertEquals("Edward Wang", form.EmailSendersNameTextBox.Text);
				AssertEquals("xwinter@cargowise.com", form.SenderEmailAddressDropEdit.Text);
				AssertEquals("", form.ReplyToTextBox.Text);

				campaign.G0_GS_NKCampaignCoordinator = "";
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
				form.G0_EmailSenderOption_ValueChanged(null, EventArgs.Empty);
				AssertEquals(true, form.EmailSendersNameTextBox.Visible);
				AssertEquals(true, form.SenderEmailAddressTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressDropEdit.Visible);
				AssertEquals(false, form.StaffAssignmentDropEdit.Visible);
				AssertEquals(false, form.SenderPoolButton.Visible);
				AssertEquals("", form.EmailSendersNameTextBox.Text);
				AssertEquals("", form.SenderEmailAddressTextBox.Text);

				campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
				form.G0_EmailSenderOption_ValueChanged(null, EventArgs.Empty);
				AssertEquals(true, form.EmailSendersNameTextBox.Visible);
				AssertEquals(true, form.SenderEmailAddressTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressDropEdit.Visible);
				AssertEquals(false, form.StaffAssignmentDropEdit.Visible);
				AssertEquals(false, form.SenderPoolButton.Visible);
				AssertEquals("Edward Wang", form.EmailSendersNameTextBox.Text);
				AssertEquals("", form.SenderEmailAddressTextBox.Text);
				AssertEquals("", form.ReplyToTextBox.Text);

				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
				form.G0_EmailSenderOption_ValueChanged(null, EventArgs.Empty);
				AssertEquals(false, form.EmailSendersNameTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressDropEdit.Visible);
				AssertEquals(true, form.StaffAssignmentDropEdit.Visible);
				AssertEquals(false, form.SenderPoolButton.Visible);
				AssertEquals("", form.ReplyToTextBox.Text);

				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
				form.G0_EmailSenderOption_ValueChanged(null, EventArgs.Empty);
				AssertEquals(false, form.EmailSendersNameTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressTextBox.Visible);
				AssertEquals(false, form.SenderEmailAddressDropEdit.Visible);
				AssertEquals(false, form.StaffAssignmentDropEdit.Visible);
				AssertEquals(true, form.SenderPoolButton.Visible);
				AssertEquals("", form.ReplyToTextBox.Text);
			}
		}

		public void TestReplyToCheckBox_CheckStateChanged()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "xwinter@cargowise.com";
			staff.GS_Code = "COR";

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				form.ReplyToCheckBox.Checked = true;
				AssertEquals("Reply to textbox should be readonly", true, form.ReplyToTextBox.ReadOnly);

				form.ReplyToCheckBox.Checked = false;
				AssertEquals("Reply to textbox should be editable", false, form.ReplyToTextBox.ReadOnly);
				AssertEquals("If Campaign Coordinator == null then ReplyTo should be blank", "", form.ReplyToTextBox.Text);

				campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;

				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
				AssertEquals("If Campaign Coordinator == null then ReplyTo should be blank", "", form.ReplyToTextBox.Text);

				form.ReplyToCheckBox.Checked = true;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
				form.ReplyToCheckBox.Checked = false;
				AssertEquals("If Campaign Coordinator is not null then ReplyTo should not be blank", "xwinter@cargowise.com", form.ReplyToTextBox.Text);

				form.ReplyToCheckBox.Checked = true;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
				campaign.G0_SenderEmail = "xwinter@cargowise.com";
				form.ReplyToCheckBox.Checked = false;
				AssertEquals("If sender email is not null when sender option is EML then ReplyTo should not be blank", "xwinter@cargowise.com", form.ReplyToTextBox.Text);
			}
		}

		public void TestSenderUnLocoCodeFindBoxOverlap()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "xwinter@cargowise.com";
			staff.GS_Code = "COR";

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				var permanentlySaveAgainstRecipientCheckBoxPosition = form.PermanentlySaveAgainstRecipientCheckBox.Location.Y;
				var senderUnLocoCodeFindBoxPosition = form.SenderUnlocoCodeFindBox.Location.Y + form.SenderUnlocoCodeFindBox.Size.Height;

				Assert("SenderUnLocoCodeFindBox should not overlap PermanentlySaveAgainstRecipientCheckBox.", permanentlySaveAgainstRecipientCheckBoxPosition > senderUnLocoCodeFindBoxPosition);
				Assert("PermanentlySaveAgainstRecipientCheckBox AutoSize should be false.", !form.PermanentlySaveAgainstRecipientCheckBox.AutoSize);
			}
		}

		#endregion

		public void TestSendScheduleChangedDialog_EmailSenderChange()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			touch.G0_EmailSenderOption = "COR";

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(touch))
			{
				form.ShowPreSaveDialogs_Exposed();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				touch.G0_EmailSenderOption = "ORG";
				form.ShowPreSaveDialogs_Exposed();
				AssertEquals("Send Schedule Changed", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Send schedule settings were changed. Select OK to recalculate all queued contacts.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendScheduleChangedDialog()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(touch))
			{
				form.ShowPreSaveDialogs_Exposed();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				touch.SendSettings.IsDelayed = true;
				form.ShowPreSaveDialogs_Exposed();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				var scheduled = touch.CampaignsItemsSent.AddNew();
				scheduled.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
				scheduled.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 2);
				form.ShowPreSaveDialogs_Exposed();
				AssertEquals("Send Schedule Changed", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Send schedule settings were changed. Select OK to recalculate all queued contacts.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2018, 11, 10)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSendScheduleChangedDialog_TypeChange()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			touch.G0_EmailSenderOption = "EML";
			touch.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch.SendSettings.IsBatchSchedule = true;
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(touch))
			{
				AssertEquals(false, touch.SendSettings.ScheduleTask.ShouldPreventNextRunTimeBounceBack);

				form.ShowPreSaveDialogs_Exposed();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				form.ShowPreSaveDialogs_Exposed();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				var queued = touch.CampaignsItemsSent.AddNew();
				queued.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;

				form.ShowPreSaveDialogs_Exposed();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				touch.SendSettings.IsFixedDate = true;
				var sendAt = new ZDateTime(2018, 11, 11, 11, 11, 0);
				touch.SendSettings.GSC_ScheduleTime = sendAt;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.ShowPreSaveDialogs_Exposed();
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasNone);

				AssertEquals(sendAt, touch.CampaignsItemsSent[0].ScheduleTime);
				AssertEquals(true, touch.SendSettings.ScheduleTask.ShouldPreventNextRunTimeBounceBack);
			}

			AssertEquals(false, touch.SendSettings.ScheduleTask.ShouldPreventNextRunTimeBounceBack);
		}

		public void TestIFrameElementsExistDialog()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			master.TemplateEditor.SetTemplateHtmlText(@"<html><body><iframe src=""""></iframe></body></html>", CampaignEmailTemplateEditor.HtmlTextSource.TextEditor);

			using (var form = GetNewGlbCompanyCampaignForm(master))
			{
				form.ShowPreSaveDialogs_Exposed();

				AssertEquals("Email Content has in-line frame elements", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("There is at least one in-line frame element in the Email Content HTML. In-line frame elements are not currently supported by most email providers and in-line frame elements typically don’t work in email. Do you want to continue the Save?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				master.TemplateEditor.SetTemplateHtmlText(@"<html><body><p>Hello World</body></html>", CampaignEmailTemplateEditor.HtmlTextSource.TextEditor);

				form.ShowPreSaveDialogs_Exposed();

				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestDeleteMasterCampaignWithTouches()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			touch.G0_EmailSenderOption = "EML";
			touch.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch.SendSettings.IsBatchSchedule = true;
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(master))
			{
				form.DisplayMode = ODisplayMode.Delete;
				AssertNoExceptionThrown(() => form.Delete_Exposed());
				AssertNoErrors(master);
			}
		}

		public void TestNewRelatedCommunicationsAreMatchedToACampaignItemIfAvailable()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			contact1.OC_Email = "andrew.luong@cargowise.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Sam";
			contact2.OC_Email = "sam@cargowise.com";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact1.PK;
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				var newRelatedCommunicationWithSameContact = Factory.New<OrgSalesCall>();
				newRelatedCommunicationWithSameContact.RelatedParentActivityPivotCollection.AddActivity(campaign);
				newRelatedCommunicationWithSameContact.OQ_OH = org.PK;
				newRelatedCommunicationWithSameContact.OQ_OC = contact1.PK;

				newRelatedCommunicationWithSameContact.Factory.Save();

				AssertContainsExactElementsInAnyOrder("Should have replaced the relationship with campaign header with its campaign item", new[] { campaignItem }, newRelatedCommunicationWithSameContact.RelatedParentActivityPivotCollection.Activities);
			}

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				var newRelatedCommunicationWithDifferentContact = Factory.New<OrgSalesCall>();
				newRelatedCommunicationWithDifferentContact.RelatedParentActivityPivotCollection.AddActivity(campaign);
				newRelatedCommunicationWithDifferentContact.OQ_OH = org.PK;
				newRelatedCommunicationWithDifferentContact.OQ_OC = contact2.PK;

				newRelatedCommunicationWithDifferentContact.Factory.Save();

				AssertContainsExactElementsInAnyOrder("Should have left the relationship with campaign header", new[] { campaign }, newRelatedCommunicationWithDifferentContact.RelatedParentActivityPivotCollection.Activities);
			}
		}

		public void TestCampaignTypeChangingEvent()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				Assert("Pre-condition", UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				AssertEquals("You have entered some Questions for the campaign. This action will remove them. Are you sure?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should be cancelled", !campaign.IsVoteCampaign);
				AssertEquals("Should not be cleared", 2, campaign.Questions.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				AssertEquals("You have entered some Questions for the campaign. This action will remove them. Are you sure?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should not be cancelled", campaign.IsVoteCampaign);
				AssertEquals("Should be cleared and replaced by VoteHeader", 1, campaign.Questions.Count);
				Assert(!campaign.Questions.Contains(question1));
				Assert(!campaign.Questions.Contains(question2));
			}
		}

		public void TestVoteSurveyPlugIns()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.VoteCampaignPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.SurveyCampaignPlugIn));
			}
		}

		public void TestFocusOnCampaignItem()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Blob";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Plonk";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item1 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			var item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;

			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				form.FocusOnCampaignItem(FocusOnTrackingTabTypes.CampaignItem, item1);
				AssertEquals("Tracking tab is selected", form.TrackingTabPage, form.MainTabControl_Exposed.SelectedTab);
				AssertEquals(1, form.CampaignTrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item1, form.CampaignTrackingControl.FilterStripControl.GridCollection);
			}
		}

		[TestDate(2015, 5, 21, 12, 12, 4)]
		public void TestFocusOnTrackingTab_And_FindItemsWithDestinationURL()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AAA";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BBB";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item1 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			var item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;

			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "http://webtest.net";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "BBB";
			link2.GCL_URL = "http://webtest.com";

			var clickTime = ZDateTime.UtcNow;

			var click11 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click11.GCC_GCL = link1.PK;
			click11.GCC_ClickTimeUtc = clickTime;
			click11.GCC_G8_Recipient = item1.PK;

			var click12 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click12.GCC_GCL = link2.PK;
			click12.GCC_ClickTimeUtc = clickTime.AddHours(4);
			click12.GCC_G8_Recipient = item1.PK;

			var click13 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click13.GCC_GCL = link2.PK;
			click13.GCC_ClickTimeUtc = clickTime;
			click13.GCC_G8_Recipient = item2.PK;

			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				form.FocusOnCampaignItem<ZString>(FocusOnTrackingTabTypes.DestinationUrl, "http://webtest.com");
				AssertEquals("Tracking tab is selected", form.TrackingTabPage, form.MainTabControl_Exposed.SelectedTab);
				AssertEquals(2, form.CampaignTrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item1, form.CampaignTrackingControl.FilterStripControl.GridCollection);
				AssertCollectionContains(item2, form.CampaignTrackingControl.FilterStripControl.GridCollection);
			}

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				form.FocusOnCampaignItem<ZString>(FocusOnTrackingTabTypes.DestinationUrl, "http://webtest.net");
				AssertEquals("Tracking tab is selected", form.TrackingTabPage, form.MainTabControl_Exposed.SelectedTab);
				AssertEquals(1, form.CampaignTrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item1, form.CampaignTrackingControl.FilterStripControl.GridCollection);
			}
		}

		public void TestFocusOnTrackingTab_WithSelectedScheduleCampaignItems()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZ";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AA";
			contact1.OC_Email = "a@b.net";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BB";
			contact2.OC_Email = "b@b.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow.Date;
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.Date;
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;

			Factory.Save();

			GlbCompanyCampaignItemSchedule scheduler = new GlbCompanyCampaignItemSchedule(campaign);
			scheduler.ScheduleItemsCollection.AddRange(campaign.CampaignsItemsSent);

			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				form.SendScheduleControl.SetDataBinding(campaign, "CampaignItemSchedule");
				form.SendScheduleControl.FilterStripControl.FirePerformSearch();
				AssertEquals(1, form.SendScheduleControl.FilterStripControl.GridCollection.Count);

				form.FocusOnCampaignItem(FocusOnTrackingTabTypes.CampaignItemList, campaign.CampaignsItemsSent);
				AssertCollectionContains(campaignItem1, form.CampaignTrackingControl.FilterStripControl.GridCollection);
				AssertCollectionContains(campaignItem2, form.CampaignTrackingControl.FilterStripControl.GridCollection);
			}
		}

		public void TestTargetListShowHideCampaignBatchCountControls()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				Assert(form.CampaignDocumentGroupBox.Visible);
				Assert(form.BatchCountCalcEdit.Visible);
				Assert(form.OrLabel.Visible);
				Assert(form.FilterForToAllContactsCheckbox.Visible);
				Assert(form.UseCampaignCheckBox.Visible);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
				Assert(!form.CampaignDocumentGroupBox.Visible);
				Assert(form.BatchCountCalcEdit.Visible);
				Assert(form.OrLabel.Visible);
				Assert(form.FilterForToAllContactsCheckbox.Visible);
				Assert(!form.UseCampaignCheckBox.Visible);
			}
		}

		public void TestEditCampaign_CampaignExists()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();

			GlbStaff campaignStaff = Factory.NewWithValidTestData<GlbStaff>();
			campaignStaff.GS_Code = "CMS";
			campaignStaff.GS_EmailAddress = "cms@test.org";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow.Date;
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;

			Factory.Save();

			OrgHeader reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			using (ZForm form = new ZForm(reloadedOrg))
			{
				using (ContactsUserControlTest.MockContactsPageControl control = new ContactsUserControlTest.MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(org, "");
					control.ContactsGrid.Select(0);
					control.ContactDetailsTabControl.SelectedIndex = 2;
					control.CampaignsGrid.Select(0);

					GlbCompanyCampaignForm activeForm = null;
					try
					{
						control.ShowCampaignDetailsForm();

						AssertEquals("Form Shown", typeof(GlbCompanyCampaignForm), control.CampaignItemController.LastShownForm.GetType());
						activeForm = (GlbCompanyCampaignForm)control.CampaignItemController.LastShownForm;
					}
					finally
					{
						if (activeForm != null)
						{
							activeForm.Close();
						}
					}
				}
			}
		}

		public void TestPermanentlySaveAgainstRecipientDefault_NewCampaign()
		{
			OrganisationsDataRegistry.Instance.PermanentlySaveAgainstRecipientDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var newCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (var form = GetNewGlbCompanyCampaignForm(newCampaign))
			{
				form.Show();
				Assert(form.PermanentlySaveAgainstRecipientCheckBox.Checked);
			}

			OrganisationsDataRegistry.Instance.PermanentlySaveAgainstRecipientDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			newCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var form = GetNewGlbCompanyCampaignForm(newCampaign))
			{
				form.Show();
				Assert(!form.PermanentlySaveAgainstRecipientCheckBox.Checked);
			}
		}

		public void TestPermanentlySaveAgainstRecipientDefault_SavedCampaign()
		{
			var campaignSaved = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			AssertEquals("Default G0_BroadcastVoteSurveyExam is BRD", campaignSaved.G0_BroadcastVoteSurveyExam, CampaignTypeList.Codes.Broadcast);
			campaignSaved.G0_StoreEmailInEDocs = true;
			Factory.Save();

			OrganisationsDataRegistry.Instance.PermanentlySaveAgainstRecipientDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = GetNewGlbCompanyCampaignForm(campaignSaved))
			{
				form.Show();
				Assert(form.PermanentlySaveAgainstRecipientCheckBox.Checked);
			}

			OrganisationsDataRegistry.Instance.PermanentlySaveAgainstRecipientDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = GetNewGlbCompanyCampaignForm(campaignSaved))
			{
				form.Show();
				Assert(form.PermanentlySaveAgainstRecipientCheckBox.Checked);
			}
		}

		public void TestSetupChartControl_PreApproachEmail()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			campaign.G0_Category = "Doc";
			campaign.G0_Type = "Jam";
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				Assert(form.TrackingStatusControl.Visible);
				Assert(!form.OpportunityCreationControl.Visible);
			}
		}

		public void TestDRMCampaign_DeleteTouch()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "teststaff@test.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1A = campaign.AllTouches.AddNew();
			touch1A.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch1A.G0_CampaignName = "Touch 1A";
			touch1A.G0_Type = "PREAP";
			touch1A.G0_Category = "PRINT";
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch1A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch1A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch1A.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			touch1A.G0_SenderEmail = "fred@test.com";
			touch1A.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch1A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
			touch1A.HtmlDocumentBlob = ZBlob.FromAscii("Test Html");
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				form.SelectTab(form.TouchesTabPage);

				var elementHost = ControlTestHelper.FindControls<ZElementHost>(form).SingleOrDefault(x => x.Name == "TouchSummaryContainer");
				var touchSummary = (TouchSummary)elementHost.Child;
				touchSummary.ViewModel.CurrentHorizontal.CurrentCampaign = touch1A;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				touchSummary.RemoveCampaign_Executed(null, null);
				Assert(!touchSummary.ViewModel.Horizontals.Any());

				var integratedTouchSummary = ControlTestHelper.FindControls<IntegratedTouchSummary>(form).SingleOrDefault(x => x.Name == "IntegratedTouchSummary");
				var trackingStatusControl = ControlTestHelper.FindControls<TrackingStatusChartUserControl>(integratedTouchSummary).SingleOrDefault(x => x.Name == "TrackingStatusControl");
				AssertNull(trackingStatusControl.StatModel.Campaign);
			}
		}

		public void TestSetupChartControl_OpportunityCreation()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			campaign.G0_Category = "Doc";
			campaign.G0_Type = "Jam";
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				Assert(form.OpportunityCreationControl.Visible);
				Assert(!form.TrackingStatusControl.Visible);
			}
		}

		public void TestDRMTransitionMasterListClick_RefreshAllCharts()
		{
			var staffCor1 = Factory.NewWithValidTestData<GlbStaff>();
			staffCor1.GS_EmailAddress = $"{nameof(staffCor1)}@test.com";
			staffCor1.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AAA";
			contact1.OC_Email = "andrew.luong@cargowise.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BBB";
			contact2.OC_Email = "fred.luong@cargowise.com";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch1A = campaign.AllTouches.AddNew();
			touch1A.G0_CampaignName = "touch 1A";
			touch1A.G0_EmailSenderOption = "EML";
			touch1A.G0_Type = "PREAP";
			touch1A.G0_Category = "PRINT";
			touch1A.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1A.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch1A.G0_GS_NKCampaignCoordinator = staffCor1.GS_Code;
			touch1A.G0_GS_NKCampaignManager = staffCor1.GS_Code;
			touch1A.G0_SenderEmail = "fred@test.com";
			touch1A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;

			var rule1 = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			rule1.GCD_G0_ParentTouch = campaign.PK;
			rule1.GCD_G0_NextTouch = touch1A.PK;

			var touch2A = campaign.AllTouches.AddNew();
			touch2A.G0_CampaignName = "touch 2A";
			touch2A.G0_EmailSenderOption = "EML";
			touch2A.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			touch2A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(2);

			var rule2 = Factory.NewWithValidTestData<GlbCompanyCampaignDripMarketing>();
			rule2.GCD_G0_ParentTouch = touch1A.PK;
			rule2.GCD_G0_NextTouch = touch2A.PK;

			Factory.Save();

			var item1 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			item1.G8_DeliveryMethod = "TGL";
			item1.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			var item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			item2.G8_DeliveryMethod = "TGL";
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;

			var link1 = touch1A.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "http://webtest.net";
			var link2 = touch1A.TrackedLinks.AddNew();
			link2.GCL_Context = "BBB";
			link2.GCL_URL = "http://webtest.com";
			var link3 = touch1A.TrackedLinks.AddNew();
			link3.GCL_Context = "BBB [Image Link]";
			link3.GCL_URL = "http://webtest.com";

			var clickTime = ZDateTime.UtcNow;

			var click11 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click11.GCC_GCL = link1.PK;
			click11.GCC_ClickTimeUtc = clickTime;

			var click12 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click12.GCC_GCL = link2.PK;
			click12.GCC_ClickTimeUtc = clickTime.AddHours(4);

			var click13 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click13.GCC_GCL = link2.PK;
			click13.GCC_ClickTimeUtc = clickTime;

			touch1A.StatModel.FromDateTime = ZDateTime.UtcNow.AddDays(-1);
			touch1A.StatModel.ToDateTime = ZDateTime.UtcNow.AddDays(2);

			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				form.SelectTab(form.TouchesTabPage);

				var transitionProgressControl = ControlTestHelper.FindControls<TransitionProgressChartUserControl>(form).SingleOrDefault(x => x.Name == "TransitionProgressControl");
#if !WINZOR
				var transitionChartHost = ControlTestHelper.FindControls<OxyplotView>(transitionProgressControl).SingleOrDefault(x => x.Name == "OxyplotControl");
				var integratedTouchSummary = ControlTestHelper.FindControls<IntegratedTouchSummary>(form).SingleOrDefault(x => x.Name == "IntegratedTouchSummary");
				var trackingStatusControl = ControlTestHelper.FindControls<TrackingStatusChartUserControl>(integratedTouchSummary).SingleOrDefault(x => x.Name == "TrackingStatusControl");

				var trackingStatusHost = ControlTestHelper.FindControls<OxyplotView>(trackingStatusControl).SingleOrDefault(x => x.Name == "OxyplotView");

				var campaignClickStatDetailsUserControl = ControlTestHelper.FindControls<CampaignClickStatDetailsUserControl>(form).SingleOrDefault(x => x.Name == "ClickStatControl");

				var summaryChartPlotView = campaignClickStatDetailsUserControl.SummaryChartPlotView;
				var summaryModel = (ClickStatModel)campaignClickStatDetailsUserControl.BindingSource.Current;
				var timeChartPlotView = campaignClickStatDetailsUserControl.TimePlotView;

				summaryModel.FromDateTime = ZDateTime.Today.AddDays(-7);
				summaryModel.ToDateTime = ZDateTime.Today.AddDays(7);
				summaryModel.ReloadLinksAndClicks();

				double totalTransitionPointValueBeforeClick = GetTotalPlotXandYForPlotModel(transitionChartHost.Model);
				double totalTrackingStatusValueBeforeClick = GetPieSlicesTotalForPlotModel(trackingStatusHost.Model);
				double totalSummaryPointValueBeforeClick = GetTotalColumnItemsForPlotModel(summaryChartPlotView.Model);
				double totalTimePointValueBeforeClick = GetTotalColumnItemsForPlotModel(timeChartPlotView.Model);

				var elementHost = ControlTestHelper.FindControls<ZElementHost>(form).SingleOrDefault(x => x.Name == "TouchSummaryContainer");
				((TouchSummary)elementHost.Child).Launch_Executed(null, null);

				double totalTransitionPointValueAfterClick = GetTotalPlotXandYForPlotModel(transitionChartHost.Model);
				double totalTrackingStatusValueAfterClick = GetPieSlicesTotalForPlotModel(trackingStatusHost.Model);
				double totalSummaryPointValueAfterClick = GetTotalColumnItemsForPlotModel(summaryChartPlotView.Model);
				double totalTimePointValueAfterClick = GetTotalColumnItemsForPlotModel(timeChartPlotView.Model);

				AssertNotEquals("Transition Point Value should change", totalTransitionPointValueBeforeClick, totalTransitionPointValueAfterClick);
				AssertNotEquals("Tracking Status pie graph should change", totalTrackingStatusValueBeforeClick, totalTrackingStatusValueAfterClick);
				AssertNotEquals("Summary Point Value should change", totalSummaryPointValueBeforeClick, totalSummaryPointValueAfterClick);
				AssertNotEquals("Time Point Value should change", totalTimePointValueBeforeClick, totalTimePointValueAfterClick);
#endif
			}
		}

		public void TestGroupBoxesDoNotOverlapWithForm()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				var trackingSummaryGroupBox = ControlTestHelper.FindControls<ZGroupBox>(form).Single(x => x.Name == "TrackingSummaryGroupBox");
				var customFieldsGroupBox = ControlTestHelper.FindControls<ZGroupBox>(form).Single(x => x.Name == "CustomFieldsGroupBox");

				AssertEquals("TrackingSummaryGroupBox Correct Size", ControlDpiScalingHelper.NewScaledSize(910, 200), trackingSummaryGroupBox.Size);
				AssertEquals("CustomFieldsGroupBox Correct Size", ControlDpiScalingHelper.NewScaledSize(479, 320), customFieldsGroupBox.Size);
			}
		}

		public void TestCampaignSummaryLabelsCanShowFiveDigitsAndComma()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "TST";
			contact.OC_Email = "test@cargowise.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				var trackingStatusControl = ControlTestHelper.FindControls<TrackingStatusChartUserControl>(form).Single(x => x.Name == "TrackingStatusControl");
				var labelTotalEmails = ControlTestHelper.FindControls<ZLabel>(trackingStatusControl).Single(x => x.Name == "labelTotalEmails");
				var labelTotalOrganizations = ControlTestHelper.FindControls<ZLabel>(trackingStatusControl).Single(x => x.Name == "labelTotalOrganizations");

				AssertEquals("LabelTotalEmails Correct Size", ControlDpiScalingHelper.NewScaledSize(50, 20), labelTotalEmails.Size);
				AssertEquals("LabelTotalOrganizations Correct Size", ControlDpiScalingHelper.NewScaledSize(50, 20), labelTotalOrganizations.Size);

				var trackingStatusChartRowUserControl = ControlTestHelper.FindControls<TrackingStatusChartRowUserControl>(trackingStatusControl).Single(x => x.Name == "TrackingStatusChartRowUserControl");
				var labelEmailsCount = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControl).Single(x => x.Name == "labelEmailsCount");
				var labelClientsCount = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControl).Single(x => x.Name == "labelClientsCount");

				AssertEquals("LabelEmailsCount Correct Size", ControlDpiScalingHelper.NewScaledSize(50, 13), labelEmailsCount.Size);
				AssertEquals("LabelClientsCount Correct Size", ControlDpiScalingHelper.NewScaledSize(50, 13), labelClientsCount.Size);
			}
		}

		public void TestCampaignSummaryLegendOppCreatedAndQueued()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "TST 1";
			contact1.OC_Email = "test1@cargowise.com";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "TST 2";
			contact2.OC_Email = "test2@cargowise.com";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "TST 3";
			contact3.OC_Email = "test3@cargowise.com";

			var contact4 = org.Contacts.AddNew();
			contact4.OC_ContactName = "TST 4";
			contact4.OC_Email = "test4@cargowise.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			campaign.AllTouches.Add(touch);

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = contact1.TablePrefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;

			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = contact2.TablePrefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;

			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = contact3.TablePrefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.OPC;

			var unsubscribe1 = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscribe1.GCS_Email = contact4.OC_Email;
			unsubscribe1.GCS_MediaCategory = campaign.G0_Category;
			unsubscribe1.GCS_MediaType = campaign.G0_Type;
			unsubscribe1.GCS_IsSubscribed = false;
			unsubscribe1.GCS_G0 = campaign.PK;
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				var trackingStatusControl = ControlTestHelper.FindControls<TrackingStatusChartUserControl>(form).Single(x => x.Name == "TrackingStatusControl");

				var trackingStatusChartRowUserControls = ControlTestHelper.FindControls<TrackingStatusChartRowUserControl>(trackingStatusControl).Where(x => x.Name == "TrackingStatusChartRowUserControl").ToArray();

				AssertEquals("Three Opportunity status rows", 3, trackingStatusChartRowUserControls.Length);

				var labelItemText1 = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControls[1]).Single(x => x.Name == "labelItemText");
				var labelEmailsCount1 = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControls[1]).Single(x => x.Name == "labelEmailsCount");
				var labelClientsCount1 = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControls[1]).Single(x => x.Name == "labelClientsCount");
				var labelRectangle1 = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControls[1]).Single(x => x.Name == "labelRectangle");

				AssertEquals("Item Text One is Opp Created", TrackingStatusCodes.Descriptions.OPC, labelItemText1.Text);
				AssertEquals("Emails Count One is 1", "1", labelEmailsCount1.Text);
				AssertEquals("Clients Count One is 1", "1", labelClientsCount1.Text);
				AssertEquals("Colour Rectangle One is LimeGreen", Color.LimeGreen, labelRectangle1.BackColor);

				var labelItemText2 = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControls[2]).Single(x => x.Name == "labelItemText");
				var labelEmailsCount2 = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControls[2]).Single(x => x.Name == "labelEmailsCount");
				var labelClientsCount2 = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControls[2]).Single(x => x.Name == "labelClientsCount");
				var labelRectangle2 = ControlTestHelper.FindControls<ZLabel>(trackingStatusChartRowUserControls[2]).Single(x => x.Name == "labelRectangle");

				AssertEquals("Item Text Two is Opp Queued", TrackingStatusCodes.Descriptions.OPQ, labelItemText2.Text);
				AssertEquals("Emails Count Two is 2", "2", labelEmailsCount2.Text);
				AssertEquals("Clients Count Two is 1", "1", labelClientsCount2.Text);
				AssertEquals("Colour Rectangle Two is IndianRed", Color.IndianRed, labelRectangle2.BackColor);
			}
		}

		public void TestPlotViewDoesNotOverlapWithParentGroupBox()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;

#if !WINZOR
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				var trackingStatusControl = ControlTestHelper.FindControls<TrackingStatusChartUserControl>(form).Single(x => x.Name == "TrackingStatusControl");
				var oxyplotView = ControlTestHelper.FindControls<OxyplotView>(trackingStatusControl).Single(x => x.Name == "OxyplotView");

				AssertEquals("OxyplotView Correct Size", ControlDpiScalingHelper.NewScaledSize(150, 110), oxyplotView.Size);
				AssertEquals("OxyplotView Correct Location", ControlDpiScalingHelper.NewScaledPoint(263, 10), oxyplotView.Location);
			}
#endif
		}

#if !WINZOR
		double GetTotalPlotXandYForPlotModel(PlotModel model)
		{
			return model.Series.OfType<DataPointSeries>().Sum(dataPointSeries => dataPointSeries.Points.Sum(point => point.X + point.Y));
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		double GetTotalColumnItemsForPlotModel(PlotModel model)
		{
			return model.Series.OfType<BarSeries>().Sum(columnSeries => columnSeries.Items.Sum(item => item.CategoryIndex * item.Value));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		double GetPieSlicesTotalForPlotModel(PlotModel model)
		{
			return model.Series.OfType<PieSeries>().Sum(pieSeries => pieSeries.Slices.Sum(slice => slice.Value));
		}

		public void TestTransitionMasterList_DeleteTransitionRules()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "teststaff@test.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "Drip Master Campaign";
			master.G0_Type = "PREAP";
			master.G0_Category = "PRINT";
			master.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			master.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			master.G0_GS_NKCampaignManager = staff.GS_Code;

			var touch1A = master.AllTouches.AddNew();
			touch1A.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch1A.G0_CampaignName = "Touch 1A";
			touch1A.G0_Type = "PREAP";
			touch1A.G0_Category = "PRINT";
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch1A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch1A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch1A.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			touch1A.G0_SenderEmail = "fred@test.com";
			touch1A.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch1A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
			touch1A.HtmlDocumentBlob = ZBlob.FromAscii("Test Html 1A");

			var touch2A = master.AllTouches.AddNew();
			touch2A.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch2A.G0_CampaignName = "Touch 2A";
			touch2A.G0_Type = "PREAP";
			touch2A.G0_Category = "PRINT";
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			touch2A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch2A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2A.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			touch2A.G0_SenderEmail = "fred@test.com";
			touch2A.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch2A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
			touch2A.HtmlDocumentBlob = ZBlob.FromAscii("Test Html 2A");

			var campaignDripMarketing = Factory.LoadTop1<GlbCompanyCampaignDripMarketing>(new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_G0_NextTouch, touch2A.PK));
			campaignDripMarketing.GCD_G0_ParentTouch = touch1A.PK;
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(master))
			{
				form.Show();
				form.SelectTab(form.TouchesTabPage);

				var elementHost = ControlTestHelper.FindControls<ZElementHost>(form).SingleOrDefault(x => x.Name == "TouchSummaryContainer");
				var touchSummary = (TouchSummary)elementHost.Child;
				AssertEquals(1, touch2A.TransitionRulesToThisCampaign.Count);

				touch2A.TransitionRulesToThisCampaign.DeleteAll();
				Factory.Save();

				AssertEquals(0, touch2A.TransitionRulesToThisCampaign.Count);
				touchSummary.Launch_Executed(null, null);

				Assert("Last message should be an error message", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("The following Touch Point(s) have no transition capability. Please set up their data source prior to transitioning the Master List:\r\nTouch 2A", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTransitionMasterList_DeleteTransitionTouch()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "teststaff@test.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "Drip Master Campaign";
			master.G0_Type = "PREAP";
			master.G0_Category = "PRINT";
			master.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			master.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			master.G0_GS_NKCampaignManager = staff.GS_Code;

			var touch1A = master.AllTouches.AddNew();
			touch1A.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch1A.G0_CampaignName = "Touch 1A";
			touch1A.G0_Type = "PREAP";
			touch1A.G0_Category = "PRINT";
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch1A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch1A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch1A.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			touch1A.G0_SenderEmail = "fred@test.com";
			touch1A.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch1A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
			touch1A.HtmlDocumentBlob = ZBlob.FromAscii("Test Html 1A");

			var touch2A = master.AllTouches.AddNew();
			touch2A.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch2A.G0_CampaignName = "Touch 2A";
			touch2A.G0_Type = "PREAP";
			touch2A.G0_Category = "PRINT";
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			touch2A.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch2A.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			touch2A.G0_GS_NKCampaignManager = staff.GS_Code;
			touch2A.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			touch2A.G0_SenderEmail = "fred@test.com";
			touch2A.G0_RL_NKEmailSenderUNLOCO = GlbBranch.GetCurrentBranch(Factory).GB_RL_NKHomePort;
			touch2A.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
			touch2A.HtmlDocumentBlob = ZBlob.FromAscii("Test Html 2A");

			var campaignDripMarketing = Factory.LoadTop1<GlbCompanyCampaignDripMarketing>(new ZQuery(GlbCompanyCampaignDripMarketingSchema.GCD_G0_NextTouch, touch2A.PK));
			campaignDripMarketing.GCD_G0_ParentTouch = touch1A.PK;
			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(master))
			{
				form.Show();
				form.SelectTab(form.TouchesTabPage);

				var elementHost = ControlTestHelper.FindControls<ZElementHost>(form).SingleOrDefault(x => x.Name == "TouchSummaryContainer");
				var touchSummary = (TouchSummary)elementHost.Child;
				AssertEquals(1, touch2A.TransitionRulesToThisCampaign.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				touchSummary.ViewModel.CurrentHorizontal.CurrentCampaign = touch1A;
				touchSummary.RemoveCampaign_Executed(null, null);

				touchSummary.Launch_Executed(null, null);
				Assert("Last message should be an error message", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Please save this campaign before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.FireSaveButton();

				Assert(master.HasErrors);
				AssertContains("The following Touch Point(s) have no transition capability:\r\nTouch 2A", master.Notifications.First().Message);
			}
		}

		public void TestEmailSenderUNLOCOValidation_DRMCampaignType()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
				campaign.Validation.ValidateAll();

				Assert(campaign.HasErrors);

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
				campaign.Validation.ValidateAll();
				Assert(campaign.G0_RL_NKEmailSenderUNLOCOInfo.HasErrors());

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
				campaign.Validation.ValidateAll();
				Assert(!campaign.G0_RL_NKEmailSenderUNLOCOInfo.HasErrors());
				AssertEquals(EmailSenderOptionCodeDescriptionList.Codes.COR, campaign.G0_EmailSenderOption);
			}
		}

		public void TestEmailSenderUNLOCOValidation_INSCampaignType()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
				campaign.Validation.ValidateAll();
				Assert(!campaign.G0_RL_NKEmailSenderUNLOCOInfo.HasErrors());

				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
				campaign.Validation.ValidateAll();
				Assert(campaign.G0_RL_NKEmailSenderUNLOCOInfo.HasErrors());

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
				campaign.Validation.ValidateAll();
				Assert(!campaign.G0_RL_NKEmailSenderUNLOCOInfo.HasErrors());
				AssertEquals(EmailSenderOptionCodeDescriptionList.Codes.COR, campaign.G0_EmailSenderOption);
			}
		}

		public void TestNoValidationErrorOnSalesPersonWhenOpportunityAssignmentChanged()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				var template = campaign.OpportunityCreationTemplate;

				template.OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson;
				template.SalesPerson = "BAD";

				form.FireSaveButton();

				Assert("Invalid SalesPerson should show error.", template.SalesPersonInfo.HasErrors());

				template.OpportunityAssignment = OpportunityAssignmentList.Codes.MatchParentTouchSender;

				form.FireSaveButton();

				Assert("Errors should be cleared when OpportunityAssignment is changed.", !template.SalesPersonInfo.HasErrors());
			}
		}

		public void TestNoValidationErrorOnStaffAssignmentWhenOpportunityAssignmentChanged()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.Show();
				var template = campaign.OpportunityCreationTemplate;

				template.OpportunityAssignment = OpportunityAssignmentList.Codes.StaffAssignment;
				template.StaffAssignment = "XYZ";

				form.FireSaveButton();

				Assert("Invalid StaffAssignment should show error.", template.StaffAssignmentInfo.HasErrors());

				template.OpportunityAssignment = OpportunityAssignmentList.Codes.MatchParentTouchSender;

				form.FireSaveButton();

				Assert("Errors should be cleared when OpportunityAssignment is changed.", !template.StaffAssignmentInfo.HasErrors());
			}
		}

		public void TestEmailSenderChanged()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;

			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			item.G8_ScheduleTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				form.ShowPreSaveDialogs_Exposed();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
				form.ShowPreSaveDialogs_Exposed();
				AssertEquals("Email Sender Changed", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Email sender was changed. Select OK to recalculate all queued contacts.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBatchCountPreventsNegativeValues()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (var form = GetNewGlbCompanyCampaignForm(campaign))
			{
				Assert("AllowNegative should be false for BatchCountCalcEdit", !form.BatchCountCalcEdit.AllowNegative);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);
		}

		protected sealed override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignForm form = GetNewGlbCompanyCampaignForm(campaign);
			form.ControllerID = ControllerIDs.GlbCompanyCampaign;
			return form;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			string formName;
			using (Form testForm = GetFormToBash())
			{
				formName = testForm.Name;

				const int MinScreenWidthSupported = 1440;
				const int MinScreenHeightSupported = 811;
				const int TypicalTaskbarHeight = 43;

				int maxSizeWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(MinScreenWidthSupported);
				int maxSizeHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(MinScreenHeightSupported - TypicalTaskbarHeight);

				Assert("Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(), testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(), testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		internal GlbCompanyCampaignFormForTest GetNewGlbCompanyCampaignForm(GlbCompanyCampaign campaign)
		{
			return new GlbCompanyCampaignFormForTest(campaign, true);
		}
		#endregion
	}
}
