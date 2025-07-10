namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LearningCentreTestSummaryForTest : LearningCentreTestSummary
	{
		public LearningCentreTestSummaryForTest(LearningCentreQuestion question)
			: base(question)
		{
		}

		public int RepliedAnswers
		{
			get { return repliedAnswers; }
		}

		public int AnsweredNumber
		{
			get { return answeredNumber; }
		}
	}
}
