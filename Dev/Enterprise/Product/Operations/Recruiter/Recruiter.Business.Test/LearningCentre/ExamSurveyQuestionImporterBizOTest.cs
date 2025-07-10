using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ExamSurveyQuestionImporterBizO))]
	sealed class ExamSurveyQuestionImporterBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldClearExistingQuestions()
		{
			Assert(!CachedBusinessObject.ShouldClearExistingQuestions);
			CachedBusinessObject.ShouldClearExistingQuestions = true;
			Assert(CachedBusinessObject.ShouldClearExistingQuestions);
		}

		public void TestCsvFileLocation()
		{
			AssertEquals(500, CachedBusinessObject.FileLocationInfo.MaxLength);
			AssertEquals("", CachedBusinessObject.FileLocation);

			CachedBusinessObject.FileLocation = Environment.Env.TempPath;
			AssertEquals(Environment.Env.TempPath, CachedBusinessObject.FileLocation);
		}

		public void TestStartingRowIndex()
		{
			AssertEquals(2, CachedBusinessObject.StartingRowIndex);
			CachedBusinessObject.StartingRowIndex = 1;
			AssertEquals(1, CachedBusinessObject.StartingRowIndex);
		}

		public void TestRunPreSaveValidation()
		{
			using (CachedBusinessObject.SuspendValidationTesting())
			{
				CachedBusinessObject.StartingRowIndex = -1;
			}
			CachedBusinessObject.RunPreSaveValidation();
			AssertEquals(2, CachedBusinessObject.Notifications.GetErrors().Count());
			Assert(CachedBusinessObject.FileLocationInfo.HasErrors());
			Assert(CachedBusinessObject.StartingRowIndexInfo.HasErrors());
		}

		public void TestStartingIndexReadOnlyIfIsXmlFile()
		{
			CachedBusinessObject.FileLocation = "import.xml";
			Assert("Should be readonly if it is a xml file", CachedBusinessObject.StartingRowIndexInfo.ReadOnly);
		}

		public void TestImportQuestions()
		{
			string csvFile = Environment.Env.GetTempFileName("", "csv");
			CachedBusinessObject.FileLocation = csvFile;
			ExamQuestionsImporter importer = ExamQuestionsImporter.New(CachedBusinessObject);

			try
			{
				NotificationBuffer notifications = new NotificationBuffer();

				using (StreamWriter writer = File.CreateText(csvFile))
				{
					writer.Write(string.Format(@"
									Question,Country,Randomisable,Comment,Correct Answer,Option1,Option2,Option3
									Capital City,AU,Y,,1,Canberra,Sydney,Melbourne
									Most Populated,,,,2,Canberra,Sydney,Melbourne
									In Victoria,ID,N,,3,Canberra,Sydney,Melbourne
									").TrimStart());
				}

				importer.importerBizO.FileLocation = csvFile;
				importer.importerBizO.StartingRowIndex = 2;
				AssertEquals(3, importer.ImportQuestions(notifications));
				AssertEquals(0, notifications.Events.Length);
				AssertEquals(3, importer.importerBizO.campaign.Questions.Count);
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[0], "Capital City", "AU", true, string.Empty, "Canberra", "Canberra", "Sydney", "Melbourne");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[1], "Most Populated", "", true, string.Empty, "Sydney", "Canberra", "Sydney", "Melbourne");
				AssertImportedQuestion(importer.importerBizO.campaign.Questions[2], "In Victoria", "ID", false, string.Empty, "Melbourne", "Canberra", "Sydney", "Melbourne");
				AssertNoErrors(importer.importerBizO.campaign);
			}
			finally
			{
				File.Delete(csvFile);
			}
		}

		void AssertImportedQuestion(VoteExamSurveyQuestion question, string questionText, string expectedCountryCode, bool isQuestionRandomisable, string comment, string correctOption, params string[] options)
		{
			AssertEquals("Question", questionText, question.HY_Question);
			AssertEquals("Options Count", options.Length, question.SubQuestions.Count);
			AssertEquals("Comment", comment, question.HY_Comment);
			foreach (LearningCentreQuestion subQuestion in question.SubQuestions)
			{
				AssertEquals("Correct question option", subQuestion.HY_Question == correctOption, subQuestion.CorrectAnswerAsBool);
			}
			AssertEquals("Country Code", expectedCountryCode, question.HY_RN_NKCountryCode);
			AssertEquals("Is Question Randomisable", isQuestionRandomisable, question.HY_IsRandomisable);
		}

		new ExamSurveyQuestionImporterBizO CachedBusinessObject
		{
			get { return (ExamSurveyQuestionImporterBizO)base.CachedBusinessObject; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return new ExamSurveyQuestionImporterBizO(campaign, new FileMapperForTest());
		}
	}
}
