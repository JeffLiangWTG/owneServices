using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.MarketingManager.WebVoting
{
	[HttpContextEnabledTest]
	class VoteControlTest : TestCaseWithFactory
	{
		public void TestSetupPagingDetailsOnLoad()
		{
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			Page.OnLoad();
			UserControl.OnLoadInternal(EventArgs.Empty);
			Assert(UserControl.PagingPanel.Visible);
			AssertEquals("4", UserControl.TotalPageLabel.Text);
			AssertEquals("1", UserControl.PagingDropDownList.SelectedValue);
			AssertEquals(4, UserControl.PagingDropDownList.Items.Count);
			for (int i = 1; i <= 4; i++)
			{
				AssertEquals(i.ToString(), UserControl.PagingDropDownList.Items[i - 1].Text);
			}
		}

		public void TestShouldDisablePagingPanelIfQuestionsFitInASinglePage()
		{
			Campaign.G0_QuestionsPerWebPage = 50;

			Page.OnLoad();
			UserControl.OnLoadInternal(EventArgs.Empty);
			UserControl.OnPreRenderInternal(EventArgs.Empty);
			Assert(!UserControl.PagingPanel.Visible);
		}

		public void TestVoteHeaderLabel()
		{
			Campaign.VoteHeader.HY_Question = "This is your chance to vote for your favourite laugh";
			Page.OnLoad();
			UserControl.OnLoadInternal(EventArgs.Empty);
			AssertEquals(Campaign.VoteHeader.HY_Question, UserControl.VoteHeaderLabel.Text);
		}

		public void TestVotingItemRepeaterBinding_RankedVote()
		{
			SetupVotingQuestionsAndAnswersForRankedVotes();

			Page.OnLoad();
			UserControl.OnLoadInternal(EventArgs.Empty);

			AssertEquals("Should be 4 pages", 4, UserControl.PagingDropDownList.Items.Count);

			AssertEquals(4, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 1, 3, "Voting Item 1c", "");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[2], 2, 5, "Voting Item 1e", "");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[3], 3, 4, "Voting Item 1d", "");

			ChangeCurrentPageAndUpdatePage(1);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 4, 1, "Voting Item 1a", "3");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[2], 5, 2, "Voting Item 1b", "");
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[3], "Header 2");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[4], 6, 6, "Voting Item 2a", "");

			ChangeCurrentPageAndUpdatePage(2);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 2 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 7, 7, "Voting Item 2b", "2");
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[2], "Header 3");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[3], 8, 10, "Voting Item 3c", "1");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[4], 9, 8, "Voting Item 3a", "");

			ChangeCurrentPageAndUpdatePage(3);
			AssertEquals(2, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 3 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 10, 9, "Voting Item 3b", "");
		}

		public void TestVotingItemRepeaterBinding_UnrankedVote()
		{
			SetupVotingQuestionsAndAnswersForUnrankedVotes();
			Page.OnLoad();
			UserControl.OnLoadInternal(EventArgs.Empty);

			AssertEquals("Should be 4 pages", 4, UserControl.PagingDropDownList.Items.Count);

			AssertEquals(4, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1");
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[1], 1, 3, "Voting Item 1c", false);
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[2], 2, 5, "Voting Item 1e", false);
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[3], 3, 4, "Voting Item 1d", false);

			ChangeCurrentPageAndUpdatePage(1);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1 (...continued)");
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[1], 4, 1, "Voting Item 1a", true);
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[2], 5, 2, "Voting Item 1b", true);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[3], "Header 2");
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[4], 6, 6, "Voting Item 2a", true);

			ChangeCurrentPageAndUpdatePage(2);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 2 (...continued)");
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[1], 7, 7, "Voting Item 2b", false);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[2], "Header 3");
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[3], 8, 10, "Voting Item 3c", true);
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[4], 9, 8, "Voting Item 3a", true);

			ChangeCurrentPageAndUpdatePage(3);
			AssertEquals(2, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 3 (...continued)");
			AssertRepeaterItemWithCheckBox(UserControl.VotingItemRepeater.Items[1], 10, 9, "Voting Item 3b", false);
		}

		public void TestVotingItemRepeaterBinding_MaxIsGreaterThan10()
		{
			SetupVotingQuestionsAndAnswersForRankedVotesWithLargeScaleRange();
			Page.OnLoad();
			UserControl.OnLoadInternal(EventArgs.Empty);

			AssertEquals("Should be 4 pages", 4, UserControl.PagingDropDownList.Items.Count);

			AssertEquals(4, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[1], 1, 3, "Voting Item 1c", "");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[2], 2, 5, "Voting Item 1e", "");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[3], 3, 4, "Voting Item 1d", "");

			ChangeCurrentPageAndUpdatePage(1);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1 (...continued)");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[1], 4, 1, "Voting Item 1a", "");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[2], 5, 2, "Voting Item 1b", "");
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[3], "Header 2");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[4], 6, 6, "Voting Item 2a", "");

			ChangeCurrentPageAndUpdatePage(2);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 2 (...continued)");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[1], 7, 7, "Voting Item 2b", "");
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[2], "Header 3");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[3], 8, 10, "Voting Item 3c", "");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[4], 9, 8, "Voting Item 3a", "12");

			ChangeCurrentPageAndUpdatePage(3);
			AssertEquals(2, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 3 (...continued)");
			AssertRepeaterItemWithNumericTextBox(UserControl.VotingItemRepeater.Items[1], 10, 9, "Voting Item 3b", "");
		}

		public void TestVotingItemRepeaterBinding_QuestionsInSequentialOrderWhenNotRandomized()
		{
			Campaign.G0_RandomizeWithinHeader = false;

			AssertEquals("Pre-condition", false, Campaign.RandomiseQuestionAndMultipleChoiceOrder);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			Page.OnLoad();
			UserControl.OnLoadInternal(EventArgs.Empty);

			AssertEquals("Should be 4 pages", 4, UserControl.PagingDropDownList.Items.Count);

			AssertEquals(4, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 1, 1, "Voting Item 1a", "", 11);
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[2], 2, 2, "Voting Item 1b", "", 11);
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[3], 3, 3, "Voting Item 1c", "", 11);

			ChangeCurrentPageAndUpdatePage(1);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 4, 4, "Voting Item 1d", "", 11);
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[2], 5, 5, "Voting Item 1e", "", 11);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[3], "Header 2");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[4], 6, 6, "Voting Item 2a", "", 11);

			ChangeCurrentPageAndUpdatePage(2);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 2 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 7, 7, "Voting Item 2b", "", 11);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[2], "Header 3");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[3], 8, 8, "Voting Item 3a", "", 11);
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[4], 9, 9, "Voting Item 3b", "", 11);

			ChangeCurrentPageAndUpdatePage(3);
			AssertEquals(2, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 3 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 10, 10, "Voting Item 3c", "", 11);
		}

		public void TestVotingItemRepeaterBinding_QuestionsInSequentialOrderWhenRandomized()
		{
			Campaign.G0_RandomizeWithinHeader = true;

			Assert("Pre-condition", Campaign.RandomiseQuestionAndMultipleChoiceOrder);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			Page.OnLoad();
			UserControl.OnLoadInternal(EventArgs.Empty);

			AssertEquals("Should be 4 pages", 4, UserControl.PagingDropDownList.Items.Count);

			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 1, 3, "Voting Item 1c", "", 11);
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[2], 2, 5, "Voting Item 1e", "", 11);
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[3], 3, 4, "Voting Item 1d", "", 11);

			ChangeCurrentPageAndUpdatePage(1);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 1 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 4, 1, "Voting Item 1a", "", 11);
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[2], 5, 2, "Voting Item 1b", "", 11);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[3], "Header 2");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[4], 6, 6, "Voting Item 2a", "", 11);

			ChangeCurrentPageAndUpdatePage(2);
			AssertEquals(5, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 2 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 7, 7, "Voting Item 2b", "", 11);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[2], "Header 3");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[3], 8, 10, "Voting Item 3c", "", 11);
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[4], 9, 8, "Voting Item 3a", "", 11);

			ChangeCurrentPageAndUpdatePage(3);
			AssertEquals(2, UserControl.VotingItemRepeater.Items.Count);
			AssertRepeaterItemWithVotingHeader(UserControl.VotingItemRepeater.Items[0], "Header 3 (...continued)");
			AssertRepeaterItemWithDropDownList(UserControl.VotingItemRepeater.Items[1], 10, 9, "Voting Item 3b", "", 11);
		}

		#region TestOverrideNew

		public void TestOverriddenNew()
		{
			AssertEquals(typeof(VoteControl), VoteControl.New(null).GetType());

			VoteControl.OverridableNewDelegate.Value = SomeSubClassVoteControl.New;
			AssertEquals(typeof(SomeSubClassVoteControl), VoteControl.New(null).GetType());
		}

		class SomeSubClassVoteControl : VoteControl
		{
			public new static SomeSubClassVoteControl New(GlbCompanyCampaign campaign)
			{
				return new SomeSubClassVoteControl();
			}

			protected SomeSubClassVoteControl()
			{
			}
		}

		#endregion

		#region Assertions

		void AssertRepeaterItemWithDropDownList(RepeaterItem repeaterItem, int votingItemNum, int questionNum, ZString votingItemText, ZString selectedValue, int numDropDownListOptions = 4)
		{
			AssertVotingItemCell(repeaterItem, votingItemNum, questionNum, votingItemText);

			HtmlTableCell answerCell = ((HtmlTableRow)repeaterItem.Controls[0]).Cells[2];
			AssertEquals("votingAnswerCell", answerCell.Attributes["class"]);
			ZDropDownList dropDownList = answerCell.Controls[0] as ZDropDownList;
			Assert(dropDownList.AutoPostBack);
			AssertEquals(selectedValue, dropDownList.SelectedValue);
			AssertEquals(numDropDownListOptions, dropDownList.Items.Count);
			AssertEquals("", dropDownList.Items[0].Text);
			AssertEquals("1", dropDownList.Items[1].Text);
			AssertEquals("2", dropDownList.Items[2].Text);
			AssertEquals("3", dropDownList.Items[3].Text);
		}

		void AssertRepeaterItemWithCheckBox(RepeaterItem repeaterItem, int votingItemNum, int questionNum, ZString votingItemText, bool checkedValue)
		{
			AssertVotingItemCell(repeaterItem, votingItemNum, questionNum, votingItemText);

			HtmlTableCell answerCell = ((HtmlTableRow)repeaterItem.Controls[0]).Cells[2];
			AssertEquals("votingAnswerCell", answerCell.Attributes["class"]);
			ZCheckBox checkBox = answerCell.Controls[0] as ZCheckBox;
			Assert(checkBox.AutoPostBack);
			AssertEquals(checkedValue, checkBox.Checked);
		}

		void AssertRepeaterItemWithNumericTextBox(RepeaterItem repeaterItem, int votingItemNum, int questionNum, ZString votingItemText, ZString assignedValue)
		{
			AssertVotingItemCell(repeaterItem, votingItemNum, questionNum, votingItemText);

			HtmlTableCell answerCell = ((HtmlTableRow)repeaterItem.Controls[0]).Cells[2];
			AssertEquals("votingAnswerCell", answerCell.Attributes["class"]);
			ZNumericTextBox textBox = answerCell.Controls[0] as ZNumericTextBox;
			Assert(textBox.AutoPostBack);
			AssertEquals(assignedValue, textBox.Text);
		}

		void AssertRepeaterItemWithVotingHeader(RepeaterItem repeaterItem, ZString votingHeaderText)
		{
			AssertEquals(1, repeaterItem.Controls.Count);

			HtmlTableRow votingItemRow = repeaterItem.Controls[0] as HtmlTableRow;
			AssertEquals("itemCategory", votingItemRow.Attributes["class"]);
			AssertEquals(1, votingItemRow.Cells.Count);
			AssertVotingItemTextContent(votingItemRow, false, votingHeaderText);
		}

		void AssertVotingItemCell(RepeaterItem repeaterItem, int displayedQuestionNum, int questionNum, ZString expectedText)
		{
			HtmlTableRow votingItemRow;
			AssertEquals(1, repeaterItem.Controls.Count);
			votingItemRow = repeaterItem.Controls[0] as HtmlTableRow;

			string expectedVotingItemClass = (repeaterItem.ItemType == ListItemType.Item) ? "votingItemRow" : "votingItemAltRow";
			AssertEquals(expectedVotingItemClass, votingItemRow.Attributes["class"]);
			AssertEquals(3, votingItemRow.Cells.Count);
			AssertVotingItemNumberAndContent(votingItemRow, displayedQuestionNum, questionNum, expectedText);
		}

		void AssertVotingItemNumberAndContent(HtmlTableRow votingItemRow, int displayedQuestionNum, int questionNum, ZString expectedText)
		{
			HtmlTableCell votingNumberCell = votingItemRow.Cells[0];
			AssertEquals(2, votingNumberCell.Controls.Count);
			AssertEquals("votingItemNumberCell", votingNumberCell.Attributes["class"]);

			AssertEquals(typeof(HiddenField), votingNumberCell.Controls[0].GetType());
			AssertEquals(questionNum.ToString(), ((HiddenField)votingNumberCell.Controls[0]).Value);

			AssertEquals(typeof(ZTextLabel), votingNumberCell.Controls[1].GetType());
			AssertEquals(displayedQuestionNum.ToString(), ((ZTextLabel)votingNumberCell.Controls[1]).Text);

			AssertVotingItemTextContent(votingItemRow, true, expectedText);
		}

		void AssertVotingItemTextContent(HtmlTableRow votingItemRow, bool hasVotingNumber, ZString expectedText)
		{
			int textContentCellIndex = (hasVotingNumber) ? 1 : 0;
			int expectedColSpan = (hasVotingNumber) ? -1 : 2;
			HtmlTableCell votingTextContentCell = votingItemRow.Cells[textContentCellIndex];
			AssertEquals(1, votingTextContentCell.Controls.Count);
			AssertEquals("When voting number is not displayed, colspan has to be set to 2", expectedColSpan, votingTextContentCell.ColSpan);
			AssertEquals("votingItemCell", votingTextContentCell.Attributes["class"]);
			AssertEquals(typeof(ZTextLabel), votingTextContentCell.Controls[0].GetType());
			AssertEquals(expectedText, ((ZTextLabel)votingTextContentCell.Controls[0]).Text);
		}

		#endregion

		void ChangeCurrentPageAndUpdatePage(int pageNumber)
		{
			UserControl.PagingDropDownList.SelectedIndex = pageNumber;
			(Page.TestDataSource as IVoteExamSurveyAnswerSet).CurrentPage = ZShort.ParseSafe(UserControl.PagingDropDownList.SelectedValue, 1);
			((IPostBackDataHandler)UserControl.PagingDropDownList).RaisePostDataChangedEvent();
			UserControl.OnLoadInternal(EventArgs.Empty);
			UserControl.OnPreRenderInternal(EventArgs.Empty);
		}

		#region Setup for test

		protected override void SetUp()
		{
			base.SetUp();

			SetupBizOsForTest();
			SetupUserControlForTest();
		}

		protected override void TearDown()
		{
			base.TearDown();

			UserControl.Dispose();
		}

		#region BizO

		void SetupBizOsForTest()
		{
			SetupCampaignAndCampaignItem();
			SetupVotingItemsAndHeaders();
			Factory.Save();
		}

		void SetupVotingQuestionsAndAnswersForRankedVotes()
		{
			Campaign.VoteHeader.HY_Min = 3;
			Campaign.VoteHeader.HY_Max = 3;
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			SetVotingAnswer("Voting Item 3c", "1");
			SetVotingAnswer("Voting Item 2b", "2");
			SetVotingAnswer("Voting Item 1a", "3");
		}

		void SetupVotingQuestionsAndAnswersForUnrankedVotes()
		{
			Campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.UnrankedVote;
			Campaign.VoteHeader.HY_Min = 5;
			Campaign.VoteHeader.HY_Max = 5;
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			SetVotingAnswer("Voting Item 3a", "Y");
			SetVotingAnswer("Voting Item 2a", "Y");
			SetVotingAnswer("Voting Item 1b", "Y");
			SetVotingAnswer("Voting Item 1a", "Y");
			SetVotingAnswer("Voting Item 3c", "Y");
		}

		void SetupVotingQuestionsAndAnswersForRankedVotesWithLargeScaleRange()
		{
			Campaign.VoteHeader.HY_Min = 1;
			Campaign.VoteHeader.HY_Max = 12;
			Campaign.G0_RandomizeWithinHeader = true;
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			SetVotingAnswer("Voting Item 3a", "12");
		}

		void SetVotingAnswer(ZString questionText, ZString answer)
		{
			foreach (VoteExamSurveyAnswerWrapper answerWrapper in (Page.TestDataSource as IVoteExamSurveyAnswerSet).AnswerWrappers)
			{
				if (answerWrapper.Question.HY_Question == questionText)
				{
					answerWrapper.SingleAnswer.HZ_Answer = answer;
					break;
				}
			}
		}

		void SetupCampaignAndCampaignItem()
		{
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.G0_QuestionsPerWebPage = 3;
			CampaignItem = Campaign.CampaignsItemsSent.AddNew();
			CampaignItem.FillWithValidTestData();
			CampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			CampaignItem.G8_RecipientID = ZGuid.NewZGuid();
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, CampaignItem);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
		}

		void SetupVotingItemsAndHeaders()
		{
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.Header, "Header 1");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 1a");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 1b");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 1c");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 1d");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 1e");

			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.Header, "Header 2");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 2a");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 2b");

			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.Header, "Header 3");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 3a");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 3b");
			AddVotingSubQuestion(VoteExamSurveyAnswerTypeList.Codes.VotingItem, "Voting Item 3c");
		}

		void AddVotingSubQuestion(ZString answerType, ZString questionText)
		{
			VoteExamSurveyQuestion question = Campaign.VoteHeader.SubQuestions.AddNew();
			question.HY_AnswerType = answerType;
			question.HY_Question = questionText;
		}

		GlbCompanyCampaign Campaign;
		GlbCompanyCampaignItem CampaignItem;
		VoteExamSurveyAnswerSet voteExamSurveyAnswerSet;

		#endregion

		void SetupUserControlForTest()
		{
			Page = new ZTestPage();

			// do not remove these BindTo checkers
			UserControl = new VoteControl();
			UserControl.Page = Page;

			Page.TestDataSource = voteExamSurveyAnswerSet;
			Page.Controls.Add(UserControl);
		}

		VoteControl UserControl;
		ZTestPage Page;

		#endregion
	}
}
