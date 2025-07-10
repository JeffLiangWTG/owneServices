using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveySystemUpgradeScheduleTaskWarningTest : TestCaseWithFactory
	{
		[TestDate(2017, 11, 14, 13, 25, 10)]
		public void TestGetWarningText()
		{
			var pageLoadedDateTimeUtc = ZDateTime.UtcNow;
			var task = Factory.New<StmScheduleTask>();

			var expected = string.Empty;
			var actual = string.Empty;

			var warning = new VoteExamSurveySystemUpgradeScheduleTaskWarning(task);

			task.S5_NextScheduledPrintRunTimeUtc = pageLoadedDateTimeUtc.AddMinutes(-60);
			expected = string.Empty;
			actual = warning.Message;
			AssertEquals("1. GetWarningText", expected, actual);

			task.S5_NextScheduledPrintRunTimeUtc = pageLoadedDateTimeUtc.AddMinutes(61);
			expected = string.Empty;
			actual = warning.Message;
			AssertEquals("2. GetWarningText", expected, actual);

			task.S5_NextScheduledPrintRunTimeUtc = pageLoadedDateTimeUtc.AddHours(26);
			expected = string.Empty;
			actual = warning.Message;
			AssertEquals("3. GetWarningText", expected, actual);

			task.S5_NextScheduledPrintRunTimeUtc = pageLoadedDateTimeUtc.AddMinutes(60);
			expected = "A scheduled system upgrade will commence in 60 minute(s). Exams in progress, that are not submitted before the upgrade, will require you to sit them again.";
			actual = warning.Message;
			AssertEquals("4. GetWarningText", expected, actual);
		}
	}
}
