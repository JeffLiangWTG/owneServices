using CargoWise.EntityFramework.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ReportFilterControlTest : TestCaseWithFactory
	{
		public void TestRunReportButton_NoPage()
		{
			var control = new ReportFilterControlForTest();

			AssertNoExceptionThrown(() => control.RunReportButton_ClickForTest());
		}
	}
}
