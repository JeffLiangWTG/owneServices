using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WebSecurityRightTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var viewRight = new WebSecurityRight("View", (NoResString)"View Description", WebSecurityApplication.EdiWebTracker);
			AssertEquals("View", viewRight.Code);
			AssertEquals("View Description", viewRight.Description);
			AssertEquals(WebSecurityApplication.EdiWebTracker, viewRight.WebApplication);
			AssertEquals(true, viewRight.IsGrantedByDefault);
			AssertEquals("View", viewRight.SecurityItemName);
			AssertEquals(ZGuid.Empty, viewRight.SecurityGuid);

			var editRight = new WebSecurityRight("Edit", (NoResString)"Edit Description", WebSecurityApplication.EdiWebTracker, false);
			AssertEquals("Edit", editRight.Code);
			AssertEquals("Edit Description", editRight.Description);
			AssertEquals(WebSecurityApplication.EdiWebTracker, editRight.WebApplication);
			AssertEquals("Should be overriden", false, editRight.IsGrantedByDefault);
			AssertEquals("Edit", editRight.SecurityItemName);
			AssertEquals(ZGuid.Empty, editRight.SecurityGuid);

			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "Test";
			report.SU_MenuName = "Test Report With A Name longer that 35 chars";
			report.SU_MenuType = "WEB";

			var reportRight = new WebSecurityRight(report.PK.ToString(), (NoResString)"Test: Test Report With A Name longer that 35 chars", WebSecurityApplication.EdiWebTracker, true);
			AssertEquals(report.PK.ToString(), reportRight.Code);
			AssertEquals("Test: Test Report With A Name longer that 35 chars", reportRight.Description);
			AssertEquals(string.Empty, reportRight.SecurityItemName);
		}
	}
}
