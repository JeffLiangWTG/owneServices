using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	public class ReapplyWorkflowTemplateMenuItemProviderTest : TestCase
	{
		#region MenuItems

		public void TestMenuItems()
		{
			module.IDOverride = ModuleIDs.WorkItem;
			module.WorkflowTypeOverride = "WKI";

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(1, menuItems.Length);

			AssertType<ReapplyWorkflowTemplateMenuItemTree>(menuItems[0]);
		}

		public void TestMenuItems_ShantShowOnUnrelated()
		{
			module.IDOverride = ModuleIDs.VisualBoard;

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(0, menuItems.Length);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			module = new DummyFilterGridModule();
			provider = new ReapplyWorkflowTemplateMenuItemProvider();
		}

		protected override void TearDown()
		{
			base.TearDown();
			module.Dispose();
		}

		DummyFilterGridModule module;
		IFilterGridMenuItemProvider provider;
	}

	public class ReapplyWorkflowTemplateMenuItemProviderTestWithFactory : TestCaseWithFactory
	{
		public void TestMenuItems_ForNonWorkflowProvider()
		{
			IFilterGridMenuItemProvider provider = new ReapplyWorkflowTemplateMenuItemProvider();

			using (var module = new DummyFilterGridModule())
			using (var form = new ZForm())
			{
				module.IDOverride = ModuleIDs.WorkItem;
				module.WorkflowTypeOverride = "WKI";

				var elementType = module.GetElementType();
				var bizo1 = Factory.NewWithValidTestData(elementType);
				var bizo2 = Factory.NewWithValidTestData(elementType);
				Factory.Save();

				var filterControl = module.EmbeddedControl as ZFilterStripControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = module.DisplayGrid;
				grid.SelectAllElements();

				var selected = module.GetSelectedBusinessObjects();
				Assert("Initially should have at least 1 selected Bizo", selected.Length > 0);

				var menuItem = provider.GetMenuItems(module).FirstOrDefault(m => m is ReapplyWorkflowTemplateMenuItemTree);

				CombineAssertions("Module is Workflow enabled, but bizo isn't a workflow provider i.e. APPayment", () =>
				{
					AssertNoExceptionThrown(menuItem.PerformClick);
					AssertMultilineASCIIEquals(@"Reapplication of workflow templates is not supported for:
NCODE - Default
NCODE - Default",
						UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestMenuItems_ShowErrorForSecurityCheckpointNotAllowed()
		{
			EnvProxy.Instance.Security.WorkflowTaskTemplatesReapply.IsAllowed = false;
			IFilterGridMenuItemProvider provider = new ReapplyWorkflowTemplateMenuItemProvider();

			using (var module = new DummyFilterGridModule())
			{
				module.IDOverride = ModuleIDs.WorkItem;
				module.WorkflowTypeOverride = "WKI";

				var menuItem = provider.GetMenuItems(module).First(m => m is ReapplyWorkflowTemplateMenuItemTree);

				CombineAssertions("Should show error message when the menu item is clicked, and the user doesn't have the permission", () =>
				{
					AssertNoExceptionThrown(menuItem.PerformClick);
					AssertEquals("ErrorMessageForNotAllowed", EnvProxy.Instance.Security.WorkflowTaskTemplatesReapply.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}
}
