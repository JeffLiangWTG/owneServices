using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreSubQuestionCollection : VoteExamSurveySubQuestionCollection
	{
		public LearningCentreSubQuestionCollection(LearningCentreQuestion question, bool isActive)
			: base(question, isActive)
		{
		}

		public new LearningCentreQuestion this[int index]
		{
			get { return (LearningCentreQuestion)base[index]; }
		}

		public new LearningCentreQuestion AddNew()
		{
			return (LearningCentreQuestion)base.AddNew();
		}
	}
}
