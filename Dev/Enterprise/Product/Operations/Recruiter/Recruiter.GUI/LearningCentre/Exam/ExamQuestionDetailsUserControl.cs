using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.GUI
{
	public partial class ExamQuestionDetailsUserControl : SurveyQuestionDetailsUserControl
	{
		public ExamQuestionDetailsUserControl()
		{
			InitializeComponent();
			this.VisibilityRelationshipProvider.SetDependency(this.MaxCalcEdit, this.MinCalcEdit);
		}

		protected override void SetupControlsVisibility()
		{
			OptionalCheckBox.Visible = false;
			base.SetupControlsVisibility();

			CorrectAnswerCalcEdit.Visible = false;
			CorrectAnswerDropEdit.Visible = false;

			LearningCentreQuestion question = (LearningCentreQuestion)CurrentDataItem;
			if (question != null)
			{
				switch (question.HY_AnswerType)
				{
					case VoteExamSurveyAnswerTypeList.Codes.NumericScale:
						CorrectAnswerCalcEdit.Visible = true;
						break;

					case VoteExamSurveyAnswerTypeList.Codes.YesNo:
					case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
						CorrectAnswerDropEdit.Visible = true;
						break;
				}
			}
		}
	}
}
