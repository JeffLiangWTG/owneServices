using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class QuestionsUserControlTest : ZFormBasherTest
	{
		public void TestPreviewButtonClick()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.FillWithValidTestData();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			using (ZForm form = GetTestForm(campaign))
			{
				form.Show();

				Button button = (Button)form.Controls.Find("PreviewButton", true)[0];
				Assert("Pre-condition", UnitTestUserNotification.Instance.LastMessage.WasNone);

				button.PerformClick();
				Assert("campaign is not saved", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please save before previewing this campaign", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				button.PerformClick();
				Assert("campaign URL has not been specified", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(UnsubscribeUrlHelper.GetUriFormatErrorMessage(campaign.Company), UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost");
				button.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				string expectedURL = VoteExamSurveyUrlHelper.GetCampaignPreviewUrl(campaign);
				AssertEquals(expectedURL, WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestQuestionsGrid_FontDecidingEvent()
		{
			TestQuestionsGrid_FontDecidingEvent("QuestionsGrid");
		}

		public void TestInactiveQuestionsGrid_FontDecidingEvent()
		{
			TestQuestionsGrid_FontDecidingEvent("InactiveQuestionsGrid");
		}

		void TestQuestionsGrid_FontDecidingEvent(string gridName)
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.FillWithValidTestData();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			using (ZForm form = GetTestForm(campaign))
			{
				ZGrid grid = (ZGrid)form.Controls.Find(gridName, true)[0];

				VoteExamSurveyQuestion headerQuestion = campaign.Questions.AddNew();
				headerQuestion.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
				VoteExamSurveyQuestion question = campaign.Questions.AddNew();
				question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;

				FieldInfo fontDecidingEventField = typeof(ZGrid).GetField("FontDeciding", BindingFlags.NonPublic | BindingFlags.Instance);
				EventHandler<FontDecidingEventArgs> fontDecidingEventHandler = (EventHandler<FontDecidingEventArgs>)fontDecidingEventField.GetValue(grid);
				FontDecidingEventArgs eventArgs = new FontDecidingEventArgs(headerQuestion, VoteExamSurveyQuestionSchema.Constants.HY_SubQuestionOrder, form.Font);
				fontDecidingEventHandler(grid, eventArgs);
				AssertNull(eventArgs.Font);

				eventArgs = new FontDecidingEventArgs(headerQuestion, VoteExamSurveyQuestionSchema.Constants.HY_Question, form.Font);
				fontDecidingEventHandler(grid, eventArgs);
				Assert(eventArgs.Font.Bold);

				eventArgs = new FontDecidingEventArgs(question, VoteExamSurveyQuestionSchema.Constants.HY_Question, form.Font);
				fontDecidingEventHandler(grid, eventArgs);
				AssertNull(eventArgs.Font);

				eventArgs = new FontDecidingEventArgs(question, VoteExamSurveyQuestionSchema.Constants.HY_Question, form.Font);
				fontDecidingEventHandler(grid, eventArgs);
				AssertNull(eventArgs.Font);
			}
		}

		public void TestQuestionDetailControlsShouldBeRefreshedAsSoonAsTheAnswerTypeDropEditIndexIsChanged()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.FillWithValidTestData();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();

			using (ZForm form = GetTestForm(campaign))
			{
				form.Show();
				Application.DoEvents();

				ZGrid questionsGrid = (ZGrid)form.Controls.Find("QuestionsGrid", true)[0];
				questionsGrid.CurrentRowIndex = 0;
				ZDropEditColumnStyle columnStyle = (ZDropEditColumnStyle)questionsGrid.TableStyles[0].GridColumnStyles[VoteExamSurveyQuestionSchema.Constants.HY_AnswerType];
				questionsGrid.BeginEdit(columnStyle, 0);
				ZDropEdit editControl = (ZDropEdit)columnStyle.EditControl;
				editControl.Text = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
				Application.DoEvents();
				AssertEquals("Should be changed immediately", VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, question.HY_AnswerType);
			}
		}

		public void TestCurrentQuestionsTabPageChanged()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.FillWithValidTestData();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			using (ZForm form = GetTestForm(campaign))
			{
				bool currentQuestionsTabPageChangedCalled = false;
				QuestionsUserControl control = (QuestionsUserControl)form.Controls.Find("QuestionsUserControl", false)[0];
				control.CurrentQuestionsTabPageChanged += delegate
				{ currentQuestionsTabPageChangedCalled = true; };
				form.Show();

				ZTabControl tabControl = (ZTabControl)control.Controls.Find("QuestionsTabControl", true)[0];
				tabControl.SelectedIndex = 1;
				Assert(currentQuestionsTabPageChangedCalled);
			}
		}

		public void TestCurrentGridBindingMember()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.FillWithValidTestData();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			using (ZForm form = GetTestForm(campaign))
			{
				QuestionsUserControl control = (QuestionsUserControl)form.Controls.Find("QuestionsUserControl", false)[0];
				form.Show();

				AssertEquals("Questions", control.CurrentGridBindingMember);
				Assert(control.IsQuestionsTabPageSelected);

				ZTabControl tabControl = (ZTabControl)control.Controls.Find("QuestionsTabControl", true)[0];
				tabControl.SelectedIndex = 1;
				AssertEquals("InactiveQuestions", control.CurrentGridBindingMember);
				Assert(control.IsQuestionsTabPageSelected);
			}
		}

		protected override Form GetFormToBashCore()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			return GetTestForm(campaign);
		}

		ZEmptyFormForBasherTest GetTestForm(GlbCompanyCampaign campaign)
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			QuestionsUserControl userControl = new QuestionsUserControl();
			userControl.Name = "QuestionsUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, null);
			result.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700);
			return result;
		}
	}
}
