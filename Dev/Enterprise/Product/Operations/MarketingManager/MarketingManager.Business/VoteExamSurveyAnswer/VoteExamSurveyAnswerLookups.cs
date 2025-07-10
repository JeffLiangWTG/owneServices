using System.Collections;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyAnswerLookups : AutoVoteExamSurveyAnswerLookups
	{
		public VoteExamSurveyAnswerLookups(AutoVoteExamSurveyAnswer parent) : base(parent)
		{
		}

		public ICollection AnswerOptionList
		{
			get { return VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Parent.Question); }
		}

		protected new VoteExamSurveyAnswer Parent
		{
			get { return (VoteExamSurveyAnswer)base.Parent; }
		}
	}
}
