using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using ZTextBox = Enterprise.ZArchitecture.Web.GUI.WebControls.ZTextBox;

namespace Enterprise.MarketingManager.WebVoting
{
	public partial class ExamSurveyUserControl : BaseUserControl
	{
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool AutoPostBack
		{
			get;
			set;
		}

		#region Paging Support

		protected void RebindQuestionsRepeater(object sender, EventArgs e)
		{
			QuestionsRepeater.DataBind();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded javascript")]
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			NextButton.Text = Res.GetString("11225b87-bb73-4ec0-a7c1-bf31b1bcec5d", "Next");
			PreviousButton.Text = Res.GetString("1b5f6650-3431-42fd-ae40-9748df64da62", "Previous");
			SubmitButton.Text = Res.GetString("76d6678b-d33b-4aa5-86aa-b17156e7fd1e", "Submit");
			JumpToPageLabel.Text = Res.GetString("69B14107-F332-4081-84D8-71A9376481BF", "Jump to Page");

			NextButton.Enabled = AnswerSet.CurrentPage < AnswerSet.PageCount;
			PreviousButton.Enabled = AnswerSet.CurrentPage > 1;
			SubmitButton.Enabled = !AnswerSet.IsPreview;
			SubmitButton.OnClientClick = string.Format("javascript:return SubmitAnswers('{0}');", AnswerSet.SubmissionConfirmationMessage);
			JumpToPageDiv.Visible = AnswerSet.PageCount > 1;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded javascript.")]
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!IsPostBack)
			{
				PageNavigator.ImageUrl = "JumpToPage.aspx?data=" + Page.DataSourceIndexer.ToString();
				SetupImageMapHotSpots();
			}
			AnswerSet.PagedAnswerWrappers.Rebuilt += RebindQuestionsRepeater;
		}

		void SetupImageMapHotSpots()
		{
			foreach (RectangleHotSpot spot in JumpToPage.GetHotSpots(AnswerSet))
			{
				PageNavigator.HotSpots.Add(spot);
			}
		}

		protected void PageNavigator_Click(object sender, ImageMapEventArgs e)
		{
			AnswerSet.CurrentPage = ZShort.ParseSafe(e.PostBackValue, (ZShort)1);
		}

		#endregion

		protected void QuestionsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				VoteExamSurveyAnswerWrapper answerWrapper = e.Item.DataItem as VoteExamSurveyAnswerWrapper;
				if (answerWrapper != null)
				{
					HtmlGenericControl questionDiv = CreateQuestionDiv(e.Item.ItemType, answerWrapper);
					e.Item.Controls.Add(questionDiv);
					new ZWebControlBinder(answerWrapper).Bind(e.Item.Controls);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant.")]
		HtmlGenericControl CreateQuestionDiv(ListItemType itemType, VoteExamSurveyAnswerWrapper answerWrapper)
		{
			HtmlGenericControl result = new HtmlGenericControl("div");

			if (answerWrapper.Question.IsHeader)
			{
				result.Attributes["class"] = "questionHeader";
				result.InnerHtml = answerWrapper.QuestionTextForWeb;
			}
			else
			{
				result.Attributes["class"] = (itemType == ListItemType.Item) ? "question" : "altQuestion";

				HtmlGenericControl questionNumberDiv = new HtmlGenericControl("div");
				questionNumberDiv.Attributes["class"] = "questionNumber";
				questionNumberDiv.InnerText = Res.GetString("d5e316ba-5be8-4972-84f9-422c04263190", "Question {0}", answerWrapper.QuestionOrder.ToString());
				result.Controls.Add(questionNumberDiv);

				HtmlGenericControl questionTextDiv = new HtmlGenericControl("div");
				questionTextDiv.Attributes["class"] = "questionText";
				questionTextDiv.InnerHtml = answerWrapper.QuestionTextForWeb;
				result.Controls.Add(questionTextDiv);

				HtmlGenericControl answerDiv = new HtmlGenericControl("div");
				answerDiv.Attributes["class"] = "answerContainer";
				IEnumerable<Control> answerControls = CreateAnswerControlDependingOnType(answerWrapper);
				foreach (Control answerControl in answerControls)
				{
					answerDiv.Controls.Add(answerControl);
				}
				result.Controls.Add(answerDiv);
			}
			return result;
		}

		IEnumerable<Control> CreateAnswerControlDependingOnType(VoteExamSurveyAnswerWrapper answerWrapper)
		{
			switch (answerWrapper.Question.HY_AnswerType)
			{
				case VoteExamSurveyAnswerTypeList.Codes.YesNo:
				case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
				case VoteExamSurveyAnswerTypeList.Codes.LikertScale:
				case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
					yield return CreateMultipleChoiceAnswerControl(answerWrapper);
					break;

				case VoteExamSurveyAnswerTypeList.Codes.Percentage:
					yield return CreateNumericScaleAnswerControl();
					yield return new LiteralControl("&nbsp;%");
					break;

				case VoteExamSurveyAnswerTypeList.Codes.NumericScale:
					yield return CreateNumericScaleAnswerControl();
					break;

				case VoteExamSurveyAnswerTypeList.Codes.FreeText:
					yield return CreateFreeTextAnswerControl();
					break;
			}
		}

		ZCheckBoxList CreateMultipleChoiceAnswerControl(VoteExamSurveyAnswerWrapper answerWrapper)
		{
			ZCheckBoxList optionList = (answerWrapper.Question.IsMultipleChoiceQuestion && answerWrapper.Question.HY_Max > 1) ? new ZCheckBoxList() : new ZRadioButtonList();
			optionList.ID = "MultipleChoiceControl";
			optionList.BindTo = "MultipleChoiceAnswerCapturer";
			optionList.AutoPostBack = AutoPostBack;
			optionList.PreRender += new EventHandler(optionList_PreRender);
			return optionList;
		}

		void optionList_PreRender(object sender, EventArgs e)
		{
			char optionMark = 'a';
			foreach (var subControl in ((ZCheckBoxList)sender).Controls)
			{
				CheckBox optionCheckBox = subControl as CheckBox;
				if (optionCheckBox != null)
				{
					optionCheckBox.Text = optionMark + ". " + optionCheckBox.Text;
					optionMark++;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		ZDropDownList CreateNumericScaleAnswerControl()
		{
			ZDropDownList numericDropDown = new ZDropDownList();
			numericDropDown.DataTextField = "Code";
			numericDropDown.BindTo = "SingleAnswer.HZ_Answer";
			numericDropDown.BindToList = "SingleAnswer.Lookups.AnswerOptionList";
			numericDropDown.AutoPostBack = AutoPostBack;
			numericDropDown.Width = 50;
			return numericDropDown;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript for textbox max length validation")]
		ZTextBox CreateFreeTextAnswerControl()
		{
			ZTextBox answerTextBox = new ZTextBox();
			answerTextBox.TextMode = TextBoxMode.MultiLine;
			answerTextBox.BindTo = "SingleAnswer.HZ_AnswerComment";
			answerTextBox.AutoPostBack = AutoPostBack;
			answerTextBox.Width = 450;
			answerTextBox.Height = 100;

			answerTextBox.Attributes["onkeypress"] = string.Format(
@"try
{{
	var normalisedText = this.value.replace(/\n/g, '\r\n').replace(/\r\r\n/g, '\r\n');
	if(normalisedText.length > ({0} - 1))
	{{
		return false; 
	}}
}} catch(e)
{{ }}", VoteExamSurveyAnswerSchema.HZ_AnswerComment.MaxLength);

			answerTextBox.Attributes["onchange"] = string.Format(
@"var normalisedText = this.value.replace(/\n/g, '\r\n').replace(/\r\r\n/g, '\r\n');
if (normalisedText.length > ({0} - 1))
{{ 
	normalisedText = normalisedText.substring(0, ({0} - 1));
}}
this.value = normalisedText.replace(/\r\n/g, '\n')", VoteExamSurveyAnswerSchema.HZ_AnswerComment.MaxLength);

			return answerTextBox;
		}

		IVoteExamSurveyAnswerSet AnswerSet
		{
			get { return Page.DataSource as IVoteExamSurveyAnswerSet; }
		}

		protected void PreviousButton_Click(object sender, EventArgs e)
		{
			if (AnswerSet.CurrentPage > 1)
			{
				AnswerSet.CurrentPage--;
			}
		}

		protected void NextButton_Click(object sender, EventArgs e)
		{
			if (AnswerSet.CurrentPage < AnswerSet.PageCount)
			{
				AnswerSet.CurrentPage++;
			}
		}

		protected void SubmitButton_Click(object sender, EventArgs e)
		{
			Default defaultPage = Page as Default;
			if (defaultPage != null)
			{
				defaultPage.HandleSubmitButtonClick();
			}
		}
	}
}
