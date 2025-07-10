using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class PackLineInspectionTypeBulkUpdatePresentationManagerTest : TestCaseWithFactory
	{
		public void TestCreateMenu_AddsMenuItemsForUserControls()
		{
			using (ZGrid dummyGrid = new ZGrid())
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				new PackLineInspectionTypeBulkUpdatePresentationManager().CreateMenus(dummyForm, dummyForm.Menu.MenuItems, dummyGrid);
				dummyForm.Show();
				var menuItem = dummyForm.Menu.MenuItems.FindByText("Set Inspection Status");
				AssertNotNull("When CreateMenu is called, the 'Set Inspection Status' menu item should be added to the menu.", menuItem);
			}
		}

		[RequiresSTA]
		public void TestCreateMenu_AddsMenuItemsForModules()
		{
			using (var form = new Form())
			using (var module = new TestDummyModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				var menuItem = module.FormActionMenu.FindByText("Set Inspection Status", true);
				AssertNotNull("'Set Inspection Status' menu item exists", menuItem);
			}
		}

		public class TestDummyModule : DummyFilterGridModule
		{
			public BusinessObject[] SelectedBusinessObjectsForTest;

			public TestDummyModule()
			{
			}

			public override BusinessObject[] GetSelectedBusinessObjects()
			{
				return SelectedBusinessObjectsForTest ?? base.GetSelectedBusinessObjects();
			}

			public ZDisplayGrid Grid_Exposed
			{
				get { return Grid; }
			}

			protected override MenuItem[] GetNewActionMenuItems()
			{
				List<MenuItem> results = new List<MenuItem>(base.GetNewActionMenuItems());
				Manager = new PackLineInspectionTypeBulkUpdatePresentationManager();
				Manager.CreateMenus(this, results);

				return results.ToArray();
			}

			public PackLineInspectionTypeBulkUpdatePresentationManager Manager { get; set; }
		}
	}
}
