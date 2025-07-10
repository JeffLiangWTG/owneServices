using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.OperationalActions.Testing
{
	[TestedType(typeof(CreateBrokerageJobOperationalActionMethodApplicator))]
	sealed class CreateBrokerageJobOperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestQuestionList()
		{
			var testApplicator = new CreateBrokerageJobOperationalActionMethodApplicatorForTest();
			AssertEquals("Question1", testApplicator.QuestionList[0].Description);
			AssertEquals("Question2", testApplicator.QuestionList[1].Description);
			AssertEquals("Question3", testApplicator.QuestionList[2].Description);
			AssertEquals("Question4", testApplicator.QuestionList[3].Description);
			AssertEquals("Question5", testApplicator.QuestionList[4].Description);
			AssertEquals("Question6", testApplicator.QuestionList[5].Description);
			Assert(testApplicator.QuestionList[0].Value);
			Assert(!testApplicator.QuestionList[1].Value);
			Assert(testApplicator.QuestionList[2].Value);
			Assert(!testApplicator.QuestionList[3].Value);
			Assert(testApplicator.QuestionList[4].Value);
			Assert(!testApplicator.QuestionList[5].Value);
			testApplicator.QuestionList[0].Value = false;
			testApplicator.QuestionList[2].Value = false;
			testApplicator.QuestionList[5].Value = true;
			var runner = testApplicator.ActionRunner_Exposed;
			Assert(!runner.AllQuestions.First(q => q.Question == "Question1").Answer);
			Assert(!runner.AllQuestions.First(q => q.Question == "Question2").Answer);
			Assert(!runner.AllQuestions.First(q => q.Question == "Question3").Answer);
			Assert(!runner.AllQuestions.First(q => q.Question == "Question4").Answer);
			Assert(runner.AllQuestions.First(q => q.Question == "Question5").Answer);
			Assert(runner.AllQuestions.First(q => q.Question == "Question6").Answer);
		}
	}
}
