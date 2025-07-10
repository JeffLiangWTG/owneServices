using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusEntryCPDecProviderTest : TestCaseWithFactory
	{
		public void TestQuestionsAndAnswersMember()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var questionsAndAnswers = declaration.QuestionsAndAnswers.ToArray();
				CombineAssertions("Case 2 | DKO Type | Has not questions", () =>
				{
					AssertEquals("Questions Count", 0, questionsAndAnswers.Length);
				});

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DTE);
				questionsAndAnswers = declaration.QuestionsAndAnswers.ToArray();
				CombineAssertions("Case 3 | DTE Type | Header Question-Answer Yes", () =>
				{
					AssertEquals("Questions Count", 3, questionsAndAnswers.Length);
					AssertEquals("LineNo", 0, questionsAndAnswers[0].LineNo);
					AssertEquals("QuestionNo", "1024", questionsAndAnswers[0].QuestionNo);
					AssertEquals("Answer", "EVET", questionsAndAnswers[0].Answer);
				});
				CombineAssertions("Case 4 | DTE Type | Detail Question-Answer Yes", () =>
				{
					AssertEquals("LineNo", 1, questionsAndAnswers[1].LineNo);
					AssertEquals("QuestionNo", "1025", questionsAndAnswers[1].QuestionNo);
					AssertEquals("Answer", "EVET", questionsAndAnswers[1].Answer);
				});
				CombineAssertions("Case 5 | DTE Type | Detail Question-Answer No", () =>
				{
					AssertEquals("LineNo", 1, questionsAndAnswers[2].LineNo);
					AssertEquals("QuestionNo", "1026", questionsAndAnswers[2].QuestionNo);
					AssertEquals("Answer", "HAYIR", questionsAndAnswers[2].Answer);
				});

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
				questionsAndAnswers = declaration.QuestionsAndAnswers.ToArray();

				AssertEquals("Questions Count", 3, questionsAndAnswers.Length);

				CombineAssertions("Case 1 | Header Question-Answer 1", () =>
				{
					AssertEquals("LineNo", 0, questionsAndAnswers[0].LineNo);
					AssertEquals("QuestionNo", "1024", questionsAndAnswers[0].QuestionNo);
					AssertEquals("Answer", "1", questionsAndAnswers[0].Answer);
				});

				CombineAssertions("Case 2 | Detail Question-Answer 1", () =>
				{
					AssertEquals("LineNo", 1, questionsAndAnswers[1].LineNo);
					AssertEquals("QuestionNo", "1025", questionsAndAnswers[1].QuestionNo);
					AssertEquals("Answer", "1", questionsAndAnswers[1].Answer);
				});

				CombineAssertions("Case 3 | Detail Question-Answer 0", () =>
				{
					AssertEquals("LineNo", 1, questionsAndAnswers[2].LineNo);
					AssertEquals("QuestionNo", "1026", questionsAndAnswers[2].QuestionNo);
					AssertEquals("Answer", "0", questionsAndAnswers[2].Answer);
				});
			}
		}
	}
}
