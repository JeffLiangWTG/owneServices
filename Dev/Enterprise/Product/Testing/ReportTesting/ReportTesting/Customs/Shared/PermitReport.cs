namespace Enterprise.ReportTesting.Customs.Shared
{
	using System.Collections.Generic;
	using Enterprise.DocumentEngine.Business;
	using Enterprise.MasterFiles.Module;

	[TemplateName("Permit Report")]
	public class TestPermitReportTemplate : TemplateTestCase
	{
	}

	public class TestPermitReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustFilesReports(); }
		}

		public override string MenuName
		{
			get { return "Permit Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Permit Report lists the Permits set up in your CargoWise system.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestPermitReportTemplate();
		}

		public new void TestMenuItemSetupCorrectly()
		{
			AssertEquals(3, CandidateMenuItems.Count);

			List<StmMenuItemBase> menuItemList = new List<StmMenuItemBase>();
			CandidateMenuItems.CopyToList(menuItemList);

			var hint = "The Permit Report lists the Permits set up in your CargoWise system. (Canada)";
			var filter = "BKRCTY=CA";
			CheckMenuItem(menuItemList, filter, hint);

			hint = "The Permit Report lists the Permits set up in your CargoWise system. (USA)";
			filter = "BKRCTY=US";
			CheckMenuItem(menuItemList, filter, hint);

			hint = "The Permit Report lists the Permits set up in your CargoWise system.";
			filter = "BKRCTY=ZA";
			CheckMenuItem(menuItemList, filter, hint);
		}

		void CheckMenuItem(List<StmMenuItemBase> menuItemList, string filter, string hint)
		{
			var menuItem = menuItemList.Find(x => x.SU_FilterList == filter);

			AssertNotNull("Must find menu item with filter: " + filter, menuItem);
			Assert("Hint must be less than 1024 characters", hint.Length <= 1024);
			AssertMultilineASCIIEquals("Incorrect Menu hint", hint, menuItem.SU_Hint);

			AssertEquals("Checking menu name of menu item with filter: " + filter, "Permit Report", menuItem.SU_MenuName);
			AssertEquals("Checking menu type of menu item with filter: " + filter, "DOC", menuItem.SU_MenuType);
			AssertEquals("Checking business context of menu item with filter: " + filter, "RepCustFilesReports", menuItem.SU_BusinessContext);
		}
	}
}
