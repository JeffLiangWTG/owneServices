using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	class WarehouseEntryFormTest : WhsGuiTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			using (var form = new WarehouseEntryFormForTest(warehouse))
			{
				AssertEquals("Posting not setup", true, form.GetSetupPostingCalled());
			}
		}

		#endregion

		#region TestFormCaption

		public void TestFormCaption()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			using (var form = new WarehouseEntryForm(warehouse))
			{
				form.Show();
				AssertEquals("Warehouse", form.FormCaption);
			}
		}

		#endregion

		#region TestLabelTips

		public void TestLabelTips()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			using (var form = new WarehouseEntryForm(warehouse))
			{
				form.Show();
				var labelTips = GuiTestHelper.FindControl<ZLabel>(form.Controls, "LabelTips");
				AssertEquals(@"Tip: To enable a Warehouse for Free Store Transactions, ensure there is at least one Area of type 'Free Store' attached to the Warehouse. All new warehouses get a Free Store Area by default.

Tip: To enable a Warehouse for Bond Transactions, ensure there is at least one Area of type 'Bond' attached to the Warehouse.

Tip: To enable a Warehouse for Excise Transactions, ensure there is at least one Area of type 'Excise' attached to the Warehouse.

Tip: To enable a Warehouse for Inward Processing Transactions, ensure there is at least one Area of type 'Inward Processing' attached to the Warehouse.

Tip: To make a Warehouse Virtual, just tick the checkbox on. You don't need to attach any Areas.", labelTips.Text);
			}
		}

		#endregion

		#region TestInwardProcessingLabel

		public void TestInwardProcessingLabel()
		{
			var warehouse = Helper.CreateWarehouse("WHS");

			using (var form = new WarehouseEntryForm(warehouse))
			{
				form.Show();

				var inwardProcessingLabel1 = GuiTestHelper.FindControl<ZLabel>(form.Controls, "InwardProcessingLabel");
				AssertEquals("Not enabled for Inward Processing", inwardProcessingLabel1.Text);
				AssertEquals(true, inwardProcessingLabel1.Visible);

				warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.InwardProcessing;
				form.Refresh();

				var inwardProcessingLabel2 = GuiTestHelper.FindControl<ZLabel>(form.Controls, "InwardProcessingLabel");
				AssertEquals("Inward Processing", inwardProcessingLabel2.Text);
				AssertEquals(true, inwardProcessingLabel2.Visible);
			}
		}

		#endregion

		#region TestVATFiscalLabel

		public void TestVATFiscalLabel()
		{
			var warehouse = Helper.CreateWarehouse("WHS");

			using (var form = new WarehouseEntryForm(warehouse))
			{
				form.Show();

				var vatFiscalLabel1 = GuiTestHelper.FindControl<ZLabel>(form.Controls, "VATFiscalLabel");
				AssertEquals("Not enabled for VAT Fiscal", vatFiscalLabel1.Text);
				AssertEquals(true, vatFiscalLabel1.Visible);

				warehouse.Areas[0].WA_AreaType = AreaTypes.Codes.VATFiscal;
				form.Refresh();

				var vatFiscalLabel2 = GuiTestHelper.FindControl<ZLabel>(form.Controls, "VATFiscalLabel");
				AssertEquals("VAT Fiscal", vatFiscalLabel2.Text);
				AssertEquals(true, vatFiscalLabel2.Visible);
			}
		}

		#endregion

		#region TestVisibleTabs

		public void TestVisibleTabs_ProductWarehouse()
		{
			TestVisibleTabsCore(WarehouseTypes.Codes.Product);
		}

		public void TestVisibleTabs_TransitWarehouse()
		{
			TestVisibleTabsCore(WarehouseTypes.Codes.Transit, workingHoursTabIsVisible: false);
		}

		public void TestVisibleTabs_FreeTradeZone()
		{
			TestVisibleTabsCore(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestVisibleTabs_ContainerYard()
		{
			TestVisibleTabsCore(WarehouseTypes.Codes.ContainerYard, undgTabIsVisible: false);
		}

		void TestVisibleTabsCore(string warehouseType, bool workingHoursTabIsVisible = true, bool undgTabIsVisible = true)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = warehouseType;

			using (var form = new WarehouseEntryForm(warehouse))
			{
				form.Show();

				AssertEquals("Entry Tab should always be visible.", true, IsEntryTabPageVisible(form));
				AssertEquals("Parameters Tab should always be visible.", true, IsParametersTabPageVisible(form));
				AssertEquals("Working Hours Tab visibilty.", workingHoursTabIsVisible, IsWorkingHoursTabPageVisible(form));
				AssertEquals("UNDG Thresholds Tab visibilty.", undgTabIsVisible, IsUNDGTabPageVisible(form));
			}
		}

		#endregion

		bool IsEntryTabPageVisible(WarehouseEntryForm form) => IsTabPageVisible(form, "MainTabPage");
		bool IsParametersTabPageVisible(WarehouseEntryForm form) => IsTabPageVisible(form, "ParametersTab");
		bool IsWorkingHoursTabPageVisible(WarehouseEntryForm form) => IsTabPageVisible(form, "WorkingHoursTabPage");
		bool IsUNDGTabPageVisible(WarehouseEntryForm form) => IsTabPageVisible(form, "UNDGTabPage");

		bool IsTabPageVisible(WarehouseEntryForm form, string tabPageName)
		{
			var tabPage = GuiTestHelper.FindControl<ZTabPage>(form.Controls, tabPageName);
			return tabPage?.TabVisible ?? false;
		}
	}

	class WarehouseEntryFormForTest : WarehouseEntryForm
	{
		public WarehouseEntryFormForTest(WhsWarehouse warehouse)
			: base(warehouse)
		{
		}

		public void SelectParametersTabPageForTest() => MainTabControl.SelectedTab = ParametersTabForTest;

		public void SelectUNDGThresholdsTabPageForTest()
		{
			var tabPage = GuiTestHelper.FindControl<ZTabPage>(Controls, "UNDGTabPage");
			MainTabControl.SelectedTab = tabPage;
		}

		public bool GetSetupPostingCalled() => SetupPostingCalled;
	}
}
