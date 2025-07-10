using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class ExportPatternMatchOverridesUtilTest : TestCaseWithFactory
	{
		public void TestMenuAddedAfterOtherPreviousMenu()
		{
			var menuParent = new MenuItem();
			var menuChild1 = new MenuItem() { Name = "menuChild1" };
			menuParent.MenuItems.Add(menuChild1);
			var menuChild2 = new MenuItem() { Name = "menuChild2" };
			menuParent.MenuItems.Add(menuChild2);
			var menuChild3 = new MenuItem() { Name = "menuChild3" };
			menuParent.MenuItems.Add(menuChild3);

			var utils = new ExportPatternMatchOverridesUtil(null);
			utils.AddExportMenuItem(menuParent, menuChild2.Name);

			menuParent.ShowPopupMenu();

			AssertEquals("New menu item added", 4, menuParent.MenuItems.Count);
			AssertEquals("Previous menu item has not changed position", "menuChild2", menuParent.MenuItems[1].Name);
			AssertEquals("New menu item added after previous menu item", "Export Pattern Match Overrides as Native XML", menuParent.MenuItems[2].Text);

			menuParent.ShowPopupMenu();

			AssertEquals("New menu not added twice", 4, menuParent.MenuItems.Count);
		}

		public void TestMenuAddedLastIfNoPreviousGiven()
		{
			var menuParent = new MenuItem();
			var menuChild1 = new MenuItem() { Name = "menuChild1" };
			menuParent.MenuItems.Add(menuChild1);
			var menuChild2 = new MenuItem() { Name = "menuChild2" };
			menuParent.MenuItems.Add(menuChild2);
			var menuChild3 = new MenuItem() { Name = "menuChild3" };
			menuParent.MenuItems.Add(menuChild3);

			var utils = new ExportPatternMatchOverridesUtil(null);
			utils.AddExportMenuItem(menuParent, null);

			menuParent.ShowPopupMenu();

			AssertEquals("New menu item added", 4, menuParent.MenuItems.Count);
			AssertEquals("New menu item added after previous menu item", "Export Pattern Match Overrides as Native XML", menuParent.MenuItems[3].Text);

			menuParent.ShowPopupMenu();

			AssertEquals("New menu not added twice", 4, menuParent.MenuItems.Count);
		}

		public void TestClickingNewMenuExportsOrgOverrides()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var orgHeader1 = factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = factory.NewWithValidTestData<OrgHeader>();
			var override1 = factory.NewWithValidTestData<OrgPatternMatchOverride>();
			var override2 = factory.NewWithValidTestData<OrgPatternMatchOverride>();
			var override3 = factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgHeader1.PatternMatchOverrides_ForBinding.Add(override1);
			orgHeader1.PatternMatchOverrides_ForBinding.Add(override2);
			orgHeader2.PatternMatchOverrides_ForBinding.Add(override3);
			factory.Save();

			var orgHeaders = new List<OrgHeader>() { orgHeader1, orgHeader2 };
			var overrides = orgHeaders.SelectMany(header => header.PatternMatchOverrides_ForBinding);

			var menuParent = new MenuItem();

			var utils = new ExportPatternMatchOverridesUtilForTest(() => orgHeaders);
			var exportService = new Mock<IExportService>();
			utils.ExportService = exportService.Object;

			utils.AddExportMenuItem(menuParent, null);

			menuParent.ShowPopupMenu();
			menuParent.MenuItems[0].PerformClick();

			AssertNoExceptionThrown("Native XML Export called with expected overrides", () => exportService.Verify(x => x.Export(overrides), Times.Once()));
		}

		public void TestDefaultExporterErrorMessageShownWhenNoOrgsProvided()
		{
			var orgHeaders = Array.Empty<OrgHeader>();

			var menuParent = new MenuItem();

			var utils = new ExportPatternMatchOverridesUtil(() => orgHeaders);

			utils.AddExportMenuItem(menuParent, null);

			menuParent.ShowPopupMenu();
			menuParent.MenuItems[0].PerformClick();

			var lastMessage = ((UnitTestUserNotification)Globals.Message).LastMessage;
			Assert("Expected error", lastMessage.WasError);
			AssertEquals("Expected error message", "Select one or more items to create the XML file for.", lastMessage.Text);
		}

		public void TestErrorMessageWhenSingleOrgHasNoOverrides()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgHeaders = new OrgHeader[] { orgHeader };

			var menuParent = new MenuItem();

			var utils = new ExportPatternMatchOverridesUtil(() => orgHeaders);

			utils.AddExportMenuItem(menuParent, null);

			menuParent.ShowPopupMenu();
			menuParent.MenuItems[0].PerformClick();

			var lastMessage = ((UnitTestUserNotification)Globals.Message).LastMessage;
			Assert("Expected error", lastMessage.WasError);
			AssertEquals("Expected error message", "No Pattern Match Overrides found on Org.", lastMessage.Text);
		}

		public void TestErrorMessageWhenMultipleOrgsHaveNoOverrides()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var orgHeader1 = factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = factory.NewWithValidTestData<OrgHeader>();
			var orgHeaders = new OrgHeader[] { orgHeader1, orgHeader2 };

			var menuParent = new MenuItem();

			var utils = new ExportPatternMatchOverridesUtil(() => orgHeaders);

			utils.AddExportMenuItem(menuParent, null);

			menuParent.ShowPopupMenu();
			menuParent.MenuItems[0].PerformClick();

			var lastMessage = ((UnitTestUserNotification)Globals.Message).LastMessage;
			Assert("Expected error", lastMessage.WasError);
			AssertEquals("Expected error message", "No Pattern Match Overrides found on Orgs.", lastMessage.Text);
		}
	}

	public class ExportPatternMatchOverridesUtilForTest : ExportPatternMatchOverridesUtil
	{
		public ExportPatternMatchOverridesUtilForTest(Func<IEnumerable<OrgHeader>> getOrgHeaders)
			: base(getOrgHeaders)
		{
			ExportService = ObjectFactory.GetDesignerSafe<IExportService>("NativeXmlExportService");
		}

		protected override IExportService GetNativeXmlExportService()
		{
			return ExportService;
		}

		public IExportService ExportService { get; set; }
	}
}
