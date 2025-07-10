using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class ExamSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEXS_IsDefault()
		{
			var exam = Factory.NewWithValidTestData<LearningCentreCampaign>();

			var settings1 = Factory.New<ExamSetting>();
			var settings2 = Factory.New<ExamSetting>();
			settings1.EXS_G0 = exam.PK;
			settings1.EXS_ExamVersion = "STD";
			settings1.EXS_IsDefault = false;
			settings2.EXS_G0 = exam.PK;
			settings2.EXS_ExamVersion = "ZZZ";
			settings2.EXS_IsDefault = false;

			settings1.Validation.ValidateEXS_IsDefault();
			settings2.Validation.ValidateEXS_IsDefault();
			AssertHasErrors(settings1.EXS_IsDefaultInfo);
			AssertHasErrors(settings2.EXS_IsDefaultInfo);

			settings1.EXS_IsDefault = true;
			settings1.Validation.ValidateEXS_IsDefault();
			settings2.Validation.ValidateEXS_IsDefault();
			AssertNoErrors(settings1.EXS_IsDefaultInfo);
			AssertNoErrors(settings2.EXS_IsDefaultInfo);

			settings2.EXS_IsDefault = true;
			settings1.Validation.ValidateEXS_IsDefault();
			settings2.Validation.ValidateEXS_IsDefault();
			AssertHasErrors(settings1.EXS_IsDefaultInfo);
			AssertHasErrors(settings2.EXS_IsDefaultInfo);
		}

		public void TestEXS_CodeEntered()
		{
			var settings = Factory.New<ExamSetting>();
			settings.Validation.ValidateEXS_Code();
			AssertHasErrors(settings.EXS_CodeInfo);

			settings.EXS_Code = "111";
			AssertNoErrors(settings.EXS_CodeInfo);
		}

		public void TestEXS_CodeUnique()
		{
			var exam = Factory.NewWithValidTestData<LearningCentreCampaign>();

			var settings1 = Factory.New<ExamSetting>();
			var settings2 = Factory.New<ExamSetting>();

			settings1.EXS_G0 = exam.PK;
			settings1.EXS_ExamVersion = "STD";
			settings2.EXS_G0 = exam.PK;
			settings2.EXS_ExamVersion = "ZZZ";

			AssertNoErrors(settings1.EXS_CodeInfo);
			AssertNoErrors(settings2.EXS_CodeInfo);

			settings2.EXS_ExamVersion = "STD";
			settings1.Validation.ValidateEXS_Code();
			AssertHasErrors(settings1.EXS_CodeInfo);
			AssertHasErrors(settings2.EXS_CodeInfo);
		}

		public void TestEXS_ExamVersion()
		{
			var settings = Factory.New<ExamSetting>();
			settings.Validation.ValidateEXS_ExamVersion();
			AssertHasErrors(settings.EXS_ExamVersionInfo);

			settings.EXS_ExamVersion = "STD";
			AssertNoErrors(settings.EXS_ExamVersionInfo);

			settings.EXS_ExamVersion = "XXX";
			AssertHasErrors(settings.EXS_ExamVersionInfo);
		}

		public void TestEXS_TestResultsExpireAfterHours()
		{
			var settings = Factory.New<ExamSetting>();
			settings.EXS_TestResultsExpireAfterHours = -5;
			AssertHasErrors(settings.EXS_TestResultsExpireAfterHoursInfo);

			settings.EXS_TestResultsExpireAfterHours = 0;
			AssertNoErrors(settings.EXS_TestResultsExpireAfterHoursInfo);

			settings.EXS_TestResultsExpireAfterHours = 5;
			AssertNoErrors(settings.EXS_TestResultsExpireAfterHoursInfo);
		}

		public void TestEXS_MaximumAskedQuestionsPerExam()
		{
			var settings = Factory.New<ExamSetting>();
			settings.EXS_MaximumAskedQuestionsPerExam = -5;
			AssertHasErrors(settings.EXS_MaximumAskedQuestionsPerExamInfo);

			settings.EXS_MaximumAskedQuestionsPerExam = 0;
			AssertHasErrors(settings.EXS_MaximumAskedQuestionsPerExamInfo);

			settings.EXS_MaximumAskedQuestionsPerExam = 5;
			AssertNoErrors(settings.EXS_MaximumAskedQuestionsPerExamInfo);
		}

		public void TestEXS_ExamExpiryTimeInMinutes()
		{
			var settings = Factory.New<ExamSetting>();
			settings.EXS_ExamExpiryTimeInMinutes = -5;
			AssertHasErrors(settings.EXS_ExamExpiryTimeInMinutesInfo);

			settings.EXS_ExamExpiryTimeInMinutes = 0;
			AssertHasErrors(settings.EXS_ExamExpiryTimeInMinutesInfo);

			settings.EXS_ExamExpiryTimeInMinutes = 5;
			AssertNoErrors(settings.EXS_ExamExpiryTimeInMinutesInfo);

			settings.EXS_ExamExpiryTimeInMinutes = 301;
			AssertHasErrors(settings.EXS_ExamExpiryTimeInMinutesInfo);
		}
	}
}
