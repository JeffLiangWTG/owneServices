using System.Collections;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreQuestionLookups : VoteExamSurveyQuestionLookups
	{
		public LearningCentreQuestionLookups(LearningCentreQuestion question)
			: base(question)
		{
		}

		public override VoteExamSurveyAnswerTypeList AnswerTypes
		{
			get { return new LearningCentreAnswerTypeList(Parent); }
		}

		public ICollection ExamCorrectAnswers
		{
			get { return VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(Parent); }
		}
	}
}
