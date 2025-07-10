using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class CsvExamQuestionsImporterTest : ExamQuestionsImporterTestCase
	{
		public void TestImportQuestions()
		{
			ExamQuestionsImporter importer = GetExamQuestionsImporter();

			string csvFile1 = Env.GetTempFileName();
			string csvFile2 = Env.GetTempFileName();
			string csvFile3 = Env.GetTempFileName();

			try
			{
				NotificationBuffer notifications = new NotificationBuffer();

				using (StreamWriter writer = File.CreateText(csvFile1))
				{
					writer.Write(string.Format(@"
						Question,Country,Randomisable,Comment,Correct Answer,Option1,Option2,Option3
						Capital City,AU,Y,,1,Canberra,Sydney,Melbourne
						Most Populated,,,,2,Canberra,Sydney,Melbourne
						In Victoria,ID,N,,3,Canberra,Sydney,Melbourne
						").TrimStart());
				}

				importer.importerBizO.FileLocation = csvFile1;
				importer.importerBizO.StartingRowIndex = 2;
				AssertEquals(3, importer.ImportQuestions(notifications));
				AssertEquals(0, notifications.Events.Length);
				AssertEquals(3, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[0], "Capital City", "AU", true, "Canberra", string.Empty, "Canberra", "Sydney", "Melbourne");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[1], "Most Populated", "", true, "Sydney", string.Empty, "Canberra", "Sydney", "Melbourne");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[2], "In Victoria", "ID", false, "Melbourne", string.Empty, "Canberra", "Sydney", "Melbourne");

				using (StreamWriter writer = File.CreateText(csvFile2))
				{
					writer.Write(string.Format(@"
						In Queensland,,,,1,Brisbane,Sydney,Melbourne
						").TrimStart());
				}
				importer.importerBizO.FileLocation = csvFile2;
				importer.importerBizO.StartingRowIndex = 1;
				AssertEquals(1, importer.ImportQuestions(notifications));
				AssertEquals(0, notifications.Events.Length);
				AssertEquals(4, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[3], "In Queensland", "", true, "Brisbane", string.Empty, "Brisbane", "Sydney", "Melbourne");

				using (StreamWriter writer = File.CreateText(csvFile3))
				{
					writer.Write(string.Format(@"
						SomeQuestionHeader
						In Tasmania,ZA,N,,1,Hobart,Sydney,Melbourne
						In Queensland,SouthAfrica,N,,3,Hobart,Sydney,Brisbane
						").TrimStart());
				}
				importer.importerBizO.FileLocation = csvFile3;
				importer.importerBizO.ShouldClearExistingQuestions = true;
				AssertEquals(3, importer.ImportQuestions(notifications));
				AssertEquals(0, notifications.Events.Length);
				AssertEquals(3, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[0], "SomeQuestionHeader");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[1], "In Tasmania", "ZA", false, "Hobart", string.Empty, "Hobart", "Sydney", "Melbourne");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[2], "In Queensland", "", false, "Brisbane", string.Empty, "Hobart", "Sydney", "Brisbane");
			}
			finally
			{
				File.Delete(csvFile1);
				File.Delete(csvFile2);
				File.Delete(csvFile3);
			}
		}

		public void TestImportQuestions_WithMultilineStrings()
		{
			var importer = GetExamQuestionsImporter();
			var csvFile = Env.GetTempFileName();

			try
			{
				var notifications = new NotificationBuffer();

				using (var writer = File.CreateText(csvFile))
				{
					writer.Write(
@"Question,Country,Randomisable,Comment,Correct Answer,Option1,Option2,Option3
How do you raise a service request (eRequest)?,,Y,,1,""Call CargoWise support desk.
And then jump."",""Get your keyboard.
Use the control and H keys on your keyboard to open the customer service search screen""");
				}

				importer.importerBizO.FileLocation = csvFile;
				importer.importerBizO.StartingRowIndex = 2;
				AssertEquals("Should import 1 question", 1, importer.ImportQuestions(notifications));
				AssertEquals(0, notifications.Events.Length);
				AssertEquals(1, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[0], "How do you raise a service request (eRequest)?", "", true,
@"Call CargoWise support desk.
And then jump.", string.Empty,
@"Call CargoWise support desk.
And then jump.",
@"Get your keyboard.
Use the control and H keys on your keyboard to open the customer service search screen");
			}
			finally
			{
				File.Delete(csvFile);
			}
		}

		public void TestImportQuestionsCheckHeaderText()
		{
			ExamQuestionsImporter importer = GetExamQuestionsImporter();

			string csvFile1 = Env.GetTempFileName();

			try
			{
				NotificationBuffer notifications = new NotificationBuffer();

				using (StreamWriter writer = File.CreateText(csvFile1))
				{
					writer.Write(string.Format(@"
						Questio,CountryCode,Randomisable,Correct Answer,Option1,Option2,Option3
						Capital City,AU,Y,1,Canberra,Sydney,Melbourne
						Most Populated,,,2,Canberra,Sydney,Melbourne
						In Victoria,ID,N,3,Canberra,Sydney,Melbourne
						").TrimStart());
				}

				importer.importerBizO.FileLocation = csvFile1;
				importer.importerBizO.StartingRowIndex = 2;
				AssertEquals(0, importer.ImportQuestions(notifications));
				AssertEquals(1, notifications.Events.Length);
				AssertEquals(0, importer.importerBizO.campaign.Questions.Count);

				var errorMessage = notifications.Events[0].Message.SplitByLine().ToArray();
				AssertEquals("Error: The CSV file contains invalid content: This file has errors in its header text:", errorMessage[0]);
				AssertEquals("Header \"Questio\" should read \"Question\"", errorMessage[1]);
				AssertEquals("Header \"CountryCode\" should read \"Country\"", errorMessage[2]);
				AssertEquals("Header \"Correct Answer\" should read \"Comment\"", errorMessage[3]);
				AssertEquals("Header \"Option1\" should read \"Correct Answer\"", errorMessage[4]);
				AssertEquals(string.Empty, errorMessage[5]);
				AssertEquals("Change these headers to match the specified text or use the template accessible from the Import Questions Form", errorMessage[6]);
			}
			finally
			{
				File.Delete(csvFile1);
			}
		}

		public void TestImportQuestionsWithComments()
		{
			ExamQuestionsImporter importer = GetExamQuestionsImporter();

			string csvFile1 = Env.GetTempFileName();

			try
			{
				NotificationBuffer notifications = new NotificationBuffer();

				using (StreamWriter writer = File.CreateText(csvFile1))
				{
					writer.Write(string.Format(@"
						Question,Country,Randomisable,Comment,Correct Answer,Option1,Option2,Option3
						Capital City,AU,Y,www.google.com.au,1,Canberra,Sydney,Melbourne
						Most Populated,,,Good to know,2,Canberra,Sydney,Melbourne
						In Victoria,ID,N,who knows,3,Canberra,Sydney,Melbourne
						").TrimStart());
				}

				importer.importerBizO.FileLocation = csvFile1;
				importer.importerBizO.StartingRowIndex = 2;
				AssertEquals(3, importer.ImportQuestions(notifications));
				AssertEquals(0, notifications.Events.Length);
				AssertEquals(3, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[0], "Capital City", "AU", true, "Canberra", "www.google.com.au", "Canberra", "Sydney", "Melbourne");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[1], "Most Populated", "", true, "Sydney", "Good to know", "Canberra", "Sydney", "Melbourne");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[2], "In Victoria", "ID", false, "Melbourne", "who knows", "Canberra", "Sydney", "Melbourne");
			}
			finally
			{
				File.Delete(csvFile1);
			}
		}

		public void TestImportQuestionsWithNoCorrectAnswer()
		{
			ExamQuestionsImporter importer = GetExamQuestionsImporter();

			string csvFile1 = Env.GetTempFileName();

			try
			{
				NotificationBuffer notifications = new NotificationBuffer();

				using (StreamWriter writer = File.CreateText(csvFile1))
				{
					writer.Write(string.Format(@"
						Question,Country,Randomisable,Comment,Correct Answer,Option1,Option2,Option3
						Capital City,AU,Y,www.google.com.au,,Canberra,Sydney,Melbourne
						Most Populated,,,Good to know,2,Canberra,Sydney,Melbourne
						In Victoria,ID,N,who knows,3,Canberra,Sydney,Melbourne
						").TrimStart());
				}

				importer.importerBizO.FileLocation = csvFile1;
				importer.importerBizO.StartingRowIndex = 2;
				AssertEquals(0, importer.ImportQuestions(notifications));
				AssertEquals(1, notifications.Events.Length);
				AssertEquals("Error: The CSV file contains invalid content: No correct answer has been supplied for question: Capital City", notifications.Events[0].Message);
			}
			finally
			{
				File.Delete(csvFile1);
			}
		}

		void AssertImportedQuestion(VoteExamSurveyQuestion question, string questionText)
		{
			AssertEquals(questionText, question.HY_Question);
			AssertEquals(0, question.SubQuestions.Count);
			AssertEquals(VoteExamSurveyAnswerTypeList.Codes.Header, question.HY_AnswerType);
		}

		void AssertImportedQuestion(VoteExamSurveyQuestion question, string questionText, string expectedCountryCode, bool isQuestionRandomisable, string correctOption, string comment, params string[] options)
		{
			AssertEquals("Question", questionText, question.HY_Question);
			AssertEquals("Options Count", options.Length, question.SubQuestions.Count);
			foreach (LearningCentreQuestion subQuestion in question.SubQuestions)
			{
				AssertEquals("Correct question option", subQuestion.HY_Question == correctOption, subQuestion.CorrectAnswerAsBool);
			}
			AssertEquals("Country Code", expectedCountryCode, question.HY_RN_NKCountryCode);
			AssertEquals("Is Question Randomisable", isQuestionRandomisable, question.HY_IsRandomisable);
			AssertEquals("Question Comment", comment, question.HY_Comment);
		}

		#region Implementation

		protected override ExamQuestionsImporter GetExamQuestionsImporter()
		{
			ExamSurveyQuestionImporterBizO importerBizO = new ExamSurveyQuestionImporterBizO(Factory.NewWithValidTestData<LearningCentreCampaign>(), new FileMapperForTest());
			importerBizO.FileLocation = "import.csv";
			return ExamQuestionsImporter.New(importerBizO);
		}

		protected override string CreateValidTestFileWithTwoQuestions()
		{
			string csvFile = Env.GetTempFileName();
			using (StreamWriter writer = File.CreateText(csvFile))
			{
				writer.Write(string.Format(@"
						Question,Country,Randomisable,Comment,Correct Answer,Option1,Option2,Option3
						Capital City,AU,Y,,1,Canberra,Sydney,Melbourne
						Most Populated,,,The greatest city on earth,2,Canberra,Sydney,Melbourne
						").TrimStart());
			}
			return csvFile;
		}

		protected override Tuple<string, string>[] InvalidTestFileContentsAndExpectedErrorMessage()
		{
			return new[]
				{
Tuple.Create(
	@"Question,Country,Randomisable,Comment,Correct Answer,Option1,Option2,Option3
	Capital City,AU,Yeah,,1,Canberra,Sydney,Melbourne
	Most Populated,,,,2,Canberra,Sydney,Melbourne",
	"Error: The CSV file contains invalid content: Cannot initialise a CargoWise.Types.ZBool with <Yeah> (System.String).")
			};
		}

		#endregion
	}
}
