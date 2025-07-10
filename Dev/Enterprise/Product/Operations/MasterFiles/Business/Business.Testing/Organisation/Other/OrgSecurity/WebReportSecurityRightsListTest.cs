using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class WebReportSecurityRightsListTest : WebSecurityRightsListTestCase
	{
		public void TestGetSecurityRightCodeForReport()
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "Test";
			report.SU_MenuName = "Test Report";
			report.SU_MenuType = "WEB";
			AssertEquals("Expected Right Code", report.PK.ToString(), ReportsWebSecurityRights.GetSecurityRightCodeAndDescriptionForReport(report).Code);
			AssertEquals("Expected Right Description", "Test: Test Report", ReportsWebSecurityRights.GetSecurityRightCodeAndDescriptionForReport(report).Description);
		}

		public void TestGetSecurityRightForReport()
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "Test";
			report.SU_MenuName = "Test Report";
			report.SU_MenuType = "WEB";

			WebSecurityRight expectedRight = ReportsWebSecurityRights.GetSecurityRightForReport(report);
			AssertNotNull("WebSecurityRight", expectedRight);

			AssertEquals("Expected Right Code", report.PK.ToString(), expectedRight.Code);
			AssertEquals("Expected Right Description", "Test: Test Report", expectedRight.Description);
			Assert("By default should not be granted right", !expectedRight.IsGrantedByDefault);
			AssertEquals(WebSecurityApplication.EdiWebTracker, expectedRight.WebApplication);
		}

		public virtual void TestConstructor()
		{
			ReportsWebSecurityRights list1 = ReportsWebSecurityRights.New(Factory);
			int existingCount = list1.Count;

			StmMenuItem report1 = GetTestMenuItem("Test1", "Test Report1");
			report1.SU_BusinessContext = "RepTestReport1";

			StmMenuItem report2 = GetTestMenuItem("Test1", "Test Report2");
			report2.SU_MenuType = "TST";
			report2.SU_BusinessContext = "RepTestReport2";

			StmMenuItem report3 = GetTestMenuItem("Test1", "Test Report3");
			report3.SU_BusinessContext = "RepTestReport3";
			report3.SU_IsPublished = false;

			StmMenuItem report4 = GetTestMenuItem("Test4", "Test Report4");
			report4.SU_BusinessContext = "TestReport4";

			ReportsWebSecurityRights list2 = ReportsWebSecurityRights.New(Factory);
			AssertEquals("Two more", existingCount + 2, list2.Count);

			WebSecurityRight firstReportRight = ReportsWebSecurityRights.GetSecurityRightForReport(report1);
			AssertNotNull("Should contain first report", firstReportRight);
			Assert("By default should not be granted right", !firstReportRight.IsGrantedByDefault);
			AssertEquals(WebSecurityApplication.EdiWebTracker, firstReportRight.WebApplication);

			WebSecurityRight thirdReportRight = ReportsWebSecurityRights.GetSecurityRightForReport(report3);
			AssertNotNull("Should contain third report", thirdReportRight);
			Assert("By default should not be granted right", !thirdReportRight.IsGrantedByDefault);
			AssertEquals(WebSecurityApplication.EdiWebTracker, thirdReportRight.WebApplication);
		}

		protected override IWebSecurityRightProvider GetNewList()
		{
			return ReportsWebSecurityRights.New(Factory);
		}

		StmMenuItem GetTestMenuItem(string context, string name)
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			report.SU_BusinessContext = context;
			report.SU_MenuName = name;
			report.SU_IsPublished = true;

			return report;
		}
	}
}
