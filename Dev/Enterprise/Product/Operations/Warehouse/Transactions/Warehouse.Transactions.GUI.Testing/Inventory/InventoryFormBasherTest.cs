using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(InventoryForm))]
	class InventoryFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override bool AllowSaveOnFormForTestHasChanges
		{
			get { return false; }
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		protected override Form GetFormToBashCore()
		{
			var data = new TestDataForInventory(Factory);

			data.Whs1 = Helper.CreateWarehouse("CP1", "A", 2, 1);
			data.Org1 = Helper.CreateClient("1");
			data.Product1 = WhsProduct.GetWhsProduct(data.Part1 = Helper.CreateProduct(data.Org1, "P1"));
			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			return new InventoryForm(receive1.Lines[0]);
		}

		#endregion

		#region TestHoldCodeChange

		public void TestHoldCodeChange()
		{
			// There were some issues with Hold Code Change checking HasChanges and the form being bound to Inventory and not DocketLine
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var docketLine = receive.Lines[0];

			using (var form = new InventoryForm(docketLine))
			{
				docketLine.IsInventoryEditForm = true;
				docketLine.HeldCodeChangeQuantity = 1m;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;

				AssertEquals("Should be able to save via the form.", true, docketLine.Inventory.HasChanges);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
				form.FireSaveButton();

				AssertEquals("Should not be prompted to save again.", false, docketLine.Inventory.HasChanges);
				AssertEquals("Should not be prompted to save again.", false, docketLine.HasChanges);

				form.FireSaveButton();
				AssertEquals("Should have only split once.", 2, Factory.Load<WhsDocketLine>(new ZQuery()).Length);
			}
		}

		#endregion

		#region TestInventory_CustomFieldsBindingToDocketLine

		public void TestInventory_CustomFieldsBindingToDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateCustomLabel(data.Org1, Constants.CustomLabels.WhsDocketLine.CustomAttribute1, "Color", false);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.Lines[0].WE_CustomAttrib1 = "RED";

			using (var form = new InventoryForm(receive.Lines[0]))
			{
				form.Show();
				var customFieldsTabPage = GUITestHelper.FindControl<ZTabPage>(form.Controls, "CustomFieldsTabPage");
				customFieldsTabPage.Show();

				var customControls = GUITestHelper.FindControl<CustomLabelsUserControl>(customFieldsTabPage.Controls, "customLabelsUserControl1");
				AssertNotNull(customControls);

				var labelControl = GUITestHelper.FindControlByText<ZLabel>(customControls.Controls, "Color: ");
				var textControl = GUITestHelper.FindControlByText<ZTextBox>(customControls.Controls, "RED");

				AssertCustomAttributeControl(labelControl, "Color: ", "", true);
				AssertCustomAttributeControl(textControl, "RED", WhsDocketLineSchema.WE_CustomAttrib1.Name, true);
			}
		}

		void AssertCustomAttributeControl(Control control, ZString text, ZString bindingMember, ZBool visible)
		{
			AssertEquals("Precondition", text, control.Text);
			AssertEquals("Precondition", bindingMember, control.GetBindingMember());
			AssertEquals("Precondition", visible, control.Visible);
		}

		#endregion

		#region TestInventory_OnInventoryPrint

		public void TestInventory_OnInventoryPrint()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			using (var form = new InventoryForm(inventory.InDocketLine))
			{
				form.Show();
				inventory.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Core.Constants.DataContext.GenericProductLabel), null);
				Assert("Should load and show WhsDocumentInventoryOptionsForm for Product Labels", ZFormModaliser.LastFormShownDialogForTest is WhsDocumentInventoryOptionsForm);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		#endregion

		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new InventoryForm(inventory.InDocketLine));
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
