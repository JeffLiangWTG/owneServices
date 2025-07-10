using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreQuestionValidation : VoteExamSurveyQuestionValidation
	{
		public LearningCentreQuestionValidation(LearningCentreQuestion question)
			: base(question)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMultipleChoiceOptions();
		}

		void ValidateMultipleChoiceOptions()
		{
			if (Parent.IsMultipleChoiceQuestion && Parent.Campaign != null && Parent.Campaign.IsExam && Parent.SubQuestions != null)
			{
				int correctAnswerCount = Parent.SubQuestions.Count(q => ((LearningCentreQuestion)q).CorrectAnswerAsBool);
				if (correctAnswerCount != Parent.HY_Max)
				{
					Parent.AddRowError(Res.GetString("f4cc89db-79ce-4e30-9ea9-8ef0ac360931", "You need to specify {0} correct answer(s)", Parent.HY_Max));
				}
			}
		}

		protected override void CheckHY_ExamCorrectAnswer()
		{
			base.CheckHY_ExamCorrectAnswer();

			if (Parent.Campaign != null
				&& Parent.Campaign.IsExam
				&& !Parent.IsSubQuestion
				&& !Parent.IsHeader
				&& !Parent.IsMultipleChoiceQuestion)
			{
				MandatoryValidation.CheckEntered(Parent.HY_ExamCorrectAnswerInfo);
				ListValidation.ErrorIfInvalidCode(Parent.HY_ExamCorrectAnswerInfo);
			}
		}

		protected override bool ShouldMandatoryValidateQuestionOrders
		{
			get { return Parent.HY_AnswerType != LearningCentreAnswerTypeList.Codes.ScaleRange; }
		}

		public new LearningCentreQuestion Parent
		{
			get { return (LearningCentreQuestion)base.Parent; }
		}
	}
}
