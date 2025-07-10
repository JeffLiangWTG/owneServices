using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LearningCentreQuestionLookupsTest : TestCaseWithFactory
	{
		public void TestAnswerTypes()
		{
			LearningCentreQuestion question = Factory.New<LearningCentreQuestion>();
			AssertEquals(typeof(LearningCentreAnswerTypeList), question.Lookups.AnswerTypes.GetType());
		}

		public void TestExamCorrectAnswers()
		{
			LearningCentreQuestion question = Factory.New<LearningCentreQuestion>();
			AssertNotNull(question.Lookups.ExamCorrectAnswers);
		}
	}
}
