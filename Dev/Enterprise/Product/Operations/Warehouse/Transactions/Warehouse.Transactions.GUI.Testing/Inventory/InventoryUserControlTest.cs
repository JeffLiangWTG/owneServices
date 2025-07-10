using System;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class InventoryUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestCaptionsAreSetupCorrectly

		public void TestCaptionsAreSetupCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, attributeName: "Colour");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, attributeName: "Size");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true, attributeName: "Batch#");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;

			using (var form = new ZForm(inventoryLine))
			{
				var control = new TestForm.TestInventoryUserControl();
				form.Controls.Add(control);
				form.Show();

				var partAttrib1TextBox = GUITestHelper.FindControl<ZTextBox>(control.Controls, "WE_PartAttrib1TextBox");
				var partAttrib2TextBox = GUITestHelper.FindControl<ZTextBox>(control.Controls, "WE_PartAttrib2TextBox");
				var partAttrib3TextBox = GUITestHelper.FindControl<ZTextBox>(control.Controls, "WE_PartAttrib3TextBox");
				var serialNumberTextBox = GUITestHelper.FindControl<ZTextBox>(control.Controls, "WE_SerialNumberTextBox");
				var packingDateEdit = GUITestHelper.FindControl<ZDateEdit>(control.Controls, "WE_PackingDateEdit");
				var expiryDateEdit = GUITestHelper.FindControl<ZDateEdit>(control.Controls, "WE_ExpiryDateEdit");

				AssertControlsVisibility(control, Visible.AllAttribs);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(14), expiryDateEdit.Top);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(38), packingDateEdit.Top);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(14), partAttrib1TextBox.Top);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(38), partAttrib2TextBox.Top);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(62), partAttrib3TextBox.Top);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(86), serialNumberTextBox.Top);
				AssertEquals(true, partAttrib1TextBox.GetExtension<ILabelCaptionRenderer>().Visible);
				AssertEquals(true, partAttrib2TextBox.GetExtension<ILabelCaptionRenderer>().Visible);
				AssertEquals(true, partAttrib3TextBox.GetExtension<ILabelCaptionRenderer>().Visible);
				AssertEquals(true, serialNumberTextBox.GetExtension<ILabelCaptionRenderer>().Visible);
				AssertEquals(true, packingDateEdit.GetExtension<ILabelCaptionRenderer>().Visible);
				AssertEquals(true, expiryDateEdit.GetExtension<ILabelCaptionRenderer>().Visible);
				AssertEquals("Colour", partAttrib1TextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Size", partAttrib2TextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Batch#", partAttrib3TextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Serial Number", serialNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Packing Date", packingDateEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Expiry Date", expiryDateEdit.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		#endregion

		public void TestSetAttributeFieldsAll()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			Helper.EnableWarehouseForBond(whs, true);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P");
			org.MiscServ.OM_IMUseExpiryDate = true;
			org.MiscServ.OM_IMUsePackingDate = true;
			org.MiscServ.OM_IMPartAttrib1Name = "Vehicle #";
			org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;
			org.MiscServ.OM_IMPartAttrib2Name = "Lot #";
			org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
			org.MiscServ.OM_IMPartAttrib3Name = "Colour";
			org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			for (int i = 1; i < 6; i++)
			{
				org.PartAttributeManager.SetProductToUseAttribute(part, i, true);
			}

			var receive = Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 5m, finalise: false);
			var inventoryLine = receive.Lines[0];
			Factory.Save();

			AssertEquals("Precondition", true, org.PartAttributeManager.IsExpiryDateUsedByProduct(part));
			AssertEquals("Precondition", true, org.PartAttributeManager.IsPackingDateUsedByProduct(part));
			for (int a = 1; a < 4; a++)
			{
				AssertEquals("Precondition", true, org.PartAttributeManager.IsPartAttributeUsedByProduct(part, a));
			}

			using (var form = new TestForm(inventoryLine))
			{
				form.Show();
				var control = form.UserControl;
				AssertControlsVisibility(control, Visible.AllAttribs ^ Visible.Serial | Visible.Bonded);

				AssertEquals("Expiry Date", control.WE_ExpiryDateEditForTest.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Packing Date", control.WE_PackingDateEditForTest.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Vehicle #", control.WE_PartAttrib1TextBoxForTest.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Lot #", control.WE_PartAttrib2TextBoxForTest.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Colour", control.WE_PartAttrib3TextBoxForTest.GetExtension<ILabelCaptionRenderer>().Caption);

				var firstControlDpiY = ControlDpiScalingHelper.ScaleToCurrentDpiY(TestForm.FirstControlTopPositionForTest);
				var scaleStepY = ControlDpiScalingHelper.ScaleToCurrentDpiY(TestForm.StepYForTest);
				AssertEquals(firstControlDpiY + scaleStepY * 0, control.WE_ExpiryDateEditForTest.Top);
				AssertEquals(firstControlDpiY + scaleStepY * 1, control.WE_PackingDateEditForTest.Top);
				AssertEquals(firstControlDpiY + scaleStepY * 2, control.WE_BondedEntryKeyTextBoxForTest.Top);
				AssertEquals(firstControlDpiY + scaleStepY * 0, control.WE_PartAttrib1TextBoxForTest.Top);
				AssertEquals(firstControlDpiY + scaleStepY * 1, control.WE_PartAttrib2TextBoxForTest.Top);
				AssertEquals(firstControlDpiY + scaleStepY * 2, control.WE_PartAttrib3TextBoxForTest.Top);

				AssertEquals(112, control.WE_ExpiryDateEditForTest.Left);
				AssertEquals(112, control.WE_PackingDateEditForTest.Left);
				AssertEquals(112, control.WE_BondedEntryKeyTextBoxForTest.Left);
				AssertEquals(406, control.WE_PartAttrib1TextBoxForTest.Left);
				AssertEquals(406, control.WE_PartAttrib2TextBoxForTest.Left);
				AssertEquals(406, control.WE_PartAttrib3TextBoxForTest.Left);

				AssertEquals(0, control.WE_ExpiryDateEditForTest.TabIndex);
				AssertEquals(1, control.WE_PackingDateEditForTest.TabIndex);
				AssertEquals(2, control.WE_BondedEntryKeyTextBoxForTest.TabIndex);
				AssertEquals(3, control.WE_PartAttrib1TextBoxForTest.TabIndex);
				AssertEquals(4, control.WE_PartAttrib2TextBoxForTest.TabIndex);
				AssertEquals(5, control.WE_PartAttrib3TextBoxForTest.TabIndex);
			}
		}

		public void TestSetAttributeFieldsRandom()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P");
			Helper.EnableWarehouseForBond(whs, true);
			org.MiscServ.OM_IMUsePackingDate = true;
			org.MiscServ.OM_IMPartAttrib2Name = "Lot #";
			org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
			org.PartAttributeManager.SetProductToUseAttribute(part, 2, true);
			org.PartAttributeManager.SetProductToUseAttribute(part, 5, true); //  packing date

			var receive = Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 5m, finalise: false);
			var inventoryLine = receive.Lines[0];
			Factory.Save();

			AssertEquals("Precondition", true, org.PartAttributeManager.IsPackingDateUsedByProduct(part));
			AssertEquals("Precondition", false, org.PartAttributeManager.IsPartAttributeUsedByProduct(part, 1));
			AssertEquals("Precondition", true, org.PartAttributeManager.IsPartAttributeUsedByProduct(part, 2));

			using (var form = new TestForm(inventoryLine))
			{
				form.Show();
				var control = form.UserControl;
				AssertControlsVisibility(control, Visible.Packing | Visible.PartAttrib2 | Visible.Bonded);

				AssertEquals("Packing Date", control.WE_PackingDateEditForTest.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Lot #", control.WE_PartAttrib2TextBoxForTest.GetExtension<ILabelCaptionRenderer>().Caption);
				var top = TestForm.FirstControlTopPositionForTest;
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(top), control.WE_PackingDateEditForTest.Top);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(top), control.WE_PartAttrib2TextBoxForTest.Top);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(top + TestForm.StepYForTest), control.WE_BondedEntryKeyTextBoxForTest.Top);
			}
		}

		public void TestSetAttributeFieldsNone()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			Helper.EnableWarehouseForBond(whs, false);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P");

			var receive = Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 5m);
			var inventoryLine = receive.Lines[0];

			using (var form = new TestForm(inventoryLine))
			{
				form.Show();
				AssertControlsVisibility(form.UserControl, Visible.None);
			}
		}

		public void TestAllocationKeyControl()
		{
			TestAllocationKeyControlCore(inwardProcessingEnabled: true);
		}

		public void TestAllocationKeyControl_CustomByProductRegistryDisabled()
		{
			TestAllocationKeyControlCore(inwardProcessingEnabled: false);
		}

		void TestAllocationKeyControlCore(bool inwardProcessingEnabled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;

			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(inwardProcessingEnabled);

			using (ObjectFactory.Substitute(mock.Object))
			using (var form = new ZForm(inventoryLine))
			{
				var control = new TestForm.TestInventoryUserControl();
				form.Controls.Add(control);
				form.Show();

				var customsGroupBox = GUITestHelper.FindControlByText<ZGroupBox>(control.Controls, "Customs Related Data");
				var allocationKeyTextBox = GUITestHelper.FindControl<ZTextBox>(customsGroupBox.Controls, "AllocationKeyTextBox");

				AssertEquals(inwardProcessingEnabled, allocationKeyTextBox.Visible);
				AssertEquals("Allocation Key", allocationKeyTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		#region Implementation

		[Flags]
		enum Visible
		{
			None = 0,
			Expiry = 1,
			Packing = 2,
			PartAttrib1 = 4,
			PartAttrib2 = 8,
			PartAttrib3 = 16,
			Serial = 32,
			Bonded = 64,
			AllAttribs = Expiry | Packing | PartAttrib1 | PartAttrib2 | PartAttrib3 | Serial
		}

		void AssertControlsVisibility(TestForm.TestInventoryUserControl control, Visible controlsVisible)
		{
			AssertEquals("WE_ExpiryDateEdit visibility", controlsVisible.HasFlag(Visible.Expiry), control.WE_ExpiryDateEditForTest.Visible);
			AssertEquals("WE_PackingDateEdit visibility", controlsVisible.HasFlag(Visible.Packing), control.WE_PackingDateEditForTest.Visible);
			AssertEquals("WE_PartAttrib1TextBox visibility", controlsVisible.HasFlag(Visible.PartAttrib1), control.WE_PartAttrib1TextBoxForTest.Visible);
			AssertEquals("WE_PartAttrib2TextBox visibility", controlsVisible.HasFlag(Visible.PartAttrib2), control.WE_PartAttrib2TextBoxForTest.Visible);
			AssertEquals("WE_PartAttrib3TextBox visibility", controlsVisible.HasFlag(Visible.PartAttrib3), control.WE_PartAttrib3TextBoxForTest.Visible);
			AssertEquals("WE_SerialNumberTextBox visibility", controlsVisible.HasFlag(Visible.Serial), control.WE_SerialNumberTextBoxForTest.Visible);
			AssertEquals("WE_BondedEntryKeyTextBox visibility", controlsVisible.HasFlag(Visible.Bonded), control.WE_BondedEntryKeyTextBoxForTest.Visible);
		}

		protected class TestForm : ZForm
		{
			public TestForm(WhsDocketLine inventoryLine) : base(inventoryLine) { }
			public TestInventoryUserControl UserControl;

			protected override void InitializeComponent()
			{
				UserControl = new TestInventoryUserControl();
				Controls.Add(this.UserControl);
				DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocketLine";
			}

			public class TestInventoryUserControl : InventoryUserControl
			{
				public ZDateEdit WE_ExpiryDateEditForTest => WE_ExpiryDateEdit;
				public ZDateEdit WE_PackingDateEditForTest => WE_PackingDateEdit;
				public ZTextBox WE_PartAttrib1TextBoxForTest => WE_PartAttrib1TextBox;
				public ZTextBox WE_PartAttrib2TextBoxForTest => WE_PartAttrib2TextBox;
				public ZTextBox WE_PartAttrib3TextBoxForTest => WE_PartAttrib3TextBox;
				public ZTextBox WE_SerialNumberTextBoxForTest => WE_SerialNumberTextBox;
				public ZTextBox WE_BondedEntryKeyTextBoxForTest => WE_BondedEntryKeyTextBox;
				public const int FirstControlTopPositionForTest = FirstControlTopPosition;
				public const int StepYForTest = StepY;
			}

			public const int FirstControlTopPositionForTest = TestInventoryUserControl.FirstControlTopPositionForTest;
			public const int StepYForTest = TestInventoryUserControl.StepYForTest;
		}

		#endregion
	}
}
