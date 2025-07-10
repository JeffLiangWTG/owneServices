using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	internal abstract class ExamQuestionsImporterTestCase : TestCaseWithFactory
	{
		public void TestNew()
		{
			ExamSurveyQuestionImporterBizO importerBizO = new ExamSurveyQuestionImporterBizO(Factory.NewWithValidTestData<GlbCompanyCampaign>(), new FileMapperForTest());

			importerBizO.FileLocation = "c:/import/";
			AssertEquals("Is a directory, return default CSV file importer", typeof(CsvExamQuestionsImporter), ExamQuestionsImporter.New(importerBizO).GetType());

			importerBizO.FileLocation = "c:/import/file.txt";
			AssertEquals("Invaild file extension, return default CSV file importer", typeof(CsvExamQuestionsImporter), ExamQuestionsImporter.New(importerBizO).GetType());

			importerBizO.FileLocation = "c:/import/file.csv";
			AssertEquals("CSV file", typeof(CsvExamQuestionsImporter), ExamQuestionsImporter.New(importerBizO).GetType());

			importerBizO.FileLocation = "c:/import/file.CSV";
			AssertEquals("CSV file extension in upper case", typeof(CsvExamQuestionsImporter), ExamQuestionsImporter.New(importerBizO).GetType());

			importerBizO.FileLocation = "c:/import/file.xml";
			AssertEquals("XML file", typeof(XmlExamQuestionsImporter), ExamQuestionsImporter.New(importerBizO).GetType());

			importerBizO.FileLocation = "c:/import/file.XML";
			AssertEquals("XML file extension in upper case", typeof(XmlExamQuestionsImporter), ExamQuestionsImporter.New(importerBizO).GetType());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestImportQuestions_NoNotificationSubscriber()
		{
			GetExamQuestionsImporter().ImportQuestions(null);
		}

		[ExpectNoExceptions]
		public void TestImportQuestions_InvalidTestFile()
		{
			var importer = GetExamQuestionsImporter();
			var invalidTestFileContentsAndExpectedErrorMessage = InvalidTestFileContentsAndExpectedErrorMessage();
			var invalidTestFiles = new string[invalidTestFileContentsAndExpectedErrorMessage.Length];

			try
			{
				CombineAssertions(() =>
				{
					for (var i = 0; i < invalidTestFileContentsAndExpectedErrorMessage.Length; i++)
					{
						invalidTestFiles[i] = Env.GetTempFileName();
						using (var writer = File.CreateText(invalidTestFiles[i]))
						{
							writer.Write(invalidTestFileContentsAndExpectedErrorMessage[i].Item1);
						}

						var notifications = new NotificationBuffer();
						importer.importerBizO.FileLocation = invalidTestFiles[i];

						AssertNoExceptionThrown(string.Format("Importing InvalidTestFile[{0}] should not throw exception", i), () => importer.ImportQuestions(notifications));
						AssertContainsExactElementsInAnyOrder(string.Format("Error messages for InvalidTestFile[{0}]", i), new[] { invalidTestFileContentsAndExpectedErrorMessage[i].Item2 }, notifications.Events.Select(e => e.Message));
					}
				});
			}
			finally
			{
				foreach (var file in invalidTestFiles)
				{
					File.Delete(file);
				}
			}
		}

		public void TestImportQuestions_IOException()
		{
			ExamQuestionsImporter importer = GetExamQuestionsImporter();
			importer.importerBizO.FileLocation = "DoesntExist.ext";
			NotificationBuffer notifications = new NotificationBuffer();
			AssertEquals(0, importer.ImportQuestions(notifications));
			AssertEquals(1, notifications.Events.Length);
			AssertEquals(ErrorType.IOError, ((ErrorNotification)notifications.Events[0]).ErrorType);
		}

		public void TestImportQuestions_ClearExistingQuestions()
		{
			ExamQuestionsImporter importer = GetExamQuestionsImporter();
			NotificationBuffer notifications = new NotificationBuffer();

			#region Prepare Test Data

			VoteExamSurveyQuestion question = importer.importerBizO.campaign.Questions.AddNew();
			question.HY_Question = "Temp question 1 should be inactivated later";
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question.HY_IsActive = true;
			VoteExamSurveyQuestion option1 = importer.importerBizO.campaign.Questions[0].SubQuestions.AddNew();
			option1.HY_SubQuestionOrder = 1;
			option1.HY_Question = "Temp question 1 option 1 should be inactivated later";
			option1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option1.HY_IsActive = true;
			VoteExamSurveyQuestion option2 = importer.importerBizO.campaign.Questions[0].SubQuestions.AddNew();
			option2.HY_SubQuestionOrder = 2;
			option2.HY_Question = "Temp question 1 option 2 should be inactivated later";
			option2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option2.HY_IsActive = true;

			#endregion

			importer.importerBizO.ShouldClearExistingQuestions = true;
			try
			{
				importer.importerBizO.FileLocation = CreateValidTestFileWithTwoQuestions();

				AssertEquals("Number of notifications", 2, importer.ImportQuestions(notifications));
				AssertEquals("Events", 0, notifications.Events.Length);
				AssertEquals("Questions number", 2, importer.importerBizO.campaign.Questions.Count);
				AssertEquals("Active questions", 2, importer.importerBizO.campaign.Questions.ActiveQuestions.Count);
				AssertEquals("Inactive questions", 1, importer.importerBizO.campaign.Questions.InactiveQuestions.Count);
				AssertEquals("Inactive questions' options", 2, importer.importerBizO.campaign.Questions.InactiveQuestions[0].InactiveSubQuestions.Count);
				AssertEquals("Inactive question 1 text", "Temp question 1 should be inactivated later", importer.importerBizO.campaign.Questions.InactiveQuestions[0].HY_Question);
				AssertEquals("Inactive question 1 option 1 text", "Temp question 1 option 1 should be inactivated later", importer.importerBizO.campaign.Questions.InactiveQuestions[0].InactiveSubQuestions[0].HY_Question);
				AssertEquals("Inactive question 1 option 2 text", "Temp question 1 option 2 should be inactivated later", importer.importerBizO.campaign.Questions.InactiveQuestions[0].InactiveSubQuestions[1].HY_Question);
			}
			finally
			{
				File.Delete(importer.importerBizO.FileLocation);
			}
		}

		protected abstract ExamQuestionsImporter GetExamQuestionsImporter();
		protected abstract string CreateValidTestFileWithTwoQuestions();
		protected abstract Tuple<string, string>[] InvalidTestFileContentsAndExpectedErrorMessage();
	}
}
