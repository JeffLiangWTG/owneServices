using System.Collections;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyQuestionLookups : AutoVoteExamSurveyQuestionLookups
	{
		public VoteExamSurveyQuestionLookups(AutoVoteExamSurveyQuestion parent) : base(parent)
		{
		}

		public ICollection CorrectAnswerOptionList
		{
			get { return VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Parent, false); }
		}

		public ICollection CorrectAnswerOptionListForNonMultipleChoiceQuestion
		{
			get { return (Parent.HY_AnswerType != VoteExamSurveyAnswerTypeList.Codes.MultipleChoice) ? CorrectAnswerOptionList : null; }
		}

		public virtual VoteExamSurveyAnswerTypeList AnswerTypes
		{
			get { return new VoteExamSurveyAnswerTypeList(Parent); }
		}

		protected new VoteExamSurveyQuestion Parent
		{
			get { return (VoteExamSurveyQuestion)base.Parent; }
		}
	}
}
