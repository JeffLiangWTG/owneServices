using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.GUI.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Warehouse.Transactions.GUI.Testing
{
	internal class InventoryFormTest : WhsGuiTestCaseWithFactory
	{
		public void TestConstructor()
		{
			using (var form = new InventoryForm(GetInventoryLine()))
			{
				AssertEquals("Posting not setup", true, form.SetupPostingCalledForTest);
				AssertNotNull("EDocs not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull("Document Menu not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull(form.ActionsMenuItemForTest.MenuItems.FindByText("View Receipt"));
				AssertNotNull(form.ActionsMenuItemForTest.MenuItems.FindByText("Committed..."));
			}
		}

		#region TestEDocsModifySetToOffNotDisablePlugin

		public void TestEDocsModifySetToOffNotDisablePlugin()
		{
			Env.Security.eDocs.IsAllowed = true;
			Env.Security.eDocsModify.IsAllowed = false;

			using (var form = new InventoryForm(GetInventoryLine()))
			{
				form.Show();
				var eDocPlugin = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				Assert("Plugin should not be disabled.", eDocPlugin.SecurityCheckpoint.IsAllowed);
			}
		}

		#endregion

		public void TestFormCaption()
		{
			using (var form = new InventoryForm(GetInventoryLine()))
			{
				AssertEquals("Inventory", form.FormCaption.Trim());
			}
		}

		public void TestOnLoad()
		{
			using (var form = new InventoryForm(GetInventoryLine()))
			{
				form.Show();

				var fileNewMenuItem = form.FileMenuItemForTest.MenuItems.FindByName(ZFormMenuStrategy.FileNewMenuItemName);
				Assert("NewAction not disabled", fileNewMenuItem == null || !fileNewMenuItem.Enabled);
			}
		}

		public void TestSupportEDocs()
		{
			using (var form = new InventoryForm(GetInventoryLine()))
			{
				AssertEquals("EDocs should be disabled", false, form.SupportsEDocsForTest);
			}
		}

		WhsDocketLine GetInventoryLine()
		{
			var receive = Factory.New<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();

			return receiveLine;
		}
	}
}
