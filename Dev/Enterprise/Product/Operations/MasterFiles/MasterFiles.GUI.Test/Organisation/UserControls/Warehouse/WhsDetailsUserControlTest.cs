using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	sealed class WhsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType() => AssertEquals(typeof(OrgHeader), userControl.BindingSource.DataSourceType);

		public void TestUseBondedWarehouseAutomationCheckBox()
		{
			var useBondedWarehouseAutomationCheckBox = userControl.UseBondedWarehouseAutomationCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", useBondedWarehouseAutomationCheckBox);
				AssertEquals("BindTo", nameof(OrgHeader.CompanyData) + "+" + nameof(OrgHeader.CompanyData.OB_IMUsedBondedWhs), useBondedWarehouseAutomationCheckBox.BindTo);
				AssertEquals("Within BondedWarehouseAutomationGroupBox", true, userControl.BondedWarehouseAutomationGroupBox.Controls.Contains(useBondedWarehouseAutomationCheckBox));
			});
		}

		public void TestEXDefaultDGContactPhoneUsedDropEdit()
		{
			var eXDefaultDGContactPhoneUsedDropEdit = userControl.EXDefaultDGContactPhoneUsedDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", eXDefaultDGContactPhoneUsedDropEdit);
				AssertEquals("BindTo", nameof(OrgHeader.MiscServ) + "+" + nameof(OrgHeader.MiscServ.OM_EXDefaultDGContactPhoneUsed), eXDefaultDGContactPhoneUsedDropEdit.BindTo);
				AssertEquals("Within DGContactDetailsGroupBox", true, userControl.DGContactDetailsGroupBox.Controls.Contains(eXDefaultDGContactPhoneUsedDropEdit));
			});
		}

		public void TestEXDefaultDGContactGuidFindBox()
		{
			var eXDefaultDGContactGuidFindBox = userControl.EXDefaultDGContactGuidFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZGuidFindBox>("Type", eXDefaultDGContactGuidFindBox);
				AssertEquals("BindTo", nameof(OrgHeader.MiscServ) + "+" + nameof(OrgHeader.MiscServ.OM_OC_EXDefaultDGContact), eXDefaultDGContactGuidFindBox.BindTo);
				AssertEquals("Within DGContactDetailsGroupBox", true, userControl.DGContactDetailsGroupBox.Controls.Contains(eXDefaultDGContactGuidFindBox));
			});
		}

		public void TestPhoneNumberzTextBox()
		{
			var phoneNumberzTextBox = userControl.PhoneNumberzTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", phoneNumberzTextBox);
				AssertEquals("BindTo", nameof(OrgHeader.MiscServ) + "+" + nameof(OrgHeader.MiscServ.DGPhoneNumber), phoneNumberzTextBox.BindTo);
				AssertEquals("Within DGContactDetailsGroupBox", true, userControl.DGContactDetailsGroupBox.Controls.Contains(phoneNumberzTextBox));
			});
		}

		public void TestIMDefaultIncoTermBoundDropEdit()
		{
			var iMDefaultIncoTermBoundDropEdit = userControl.IMDefaultIncoTermBoundDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", iMDefaultIncoTermBoundDropEdit);
				AssertEquals("BindTo", nameof(OrgHeader.MiscServ) + "+" + nameof(OrgHeader.MiscServ.OM_IMDefaultINCOTerm), iMDefaultIncoTermBoundDropEdit.BindTo);
				AssertEquals("Within ConfigurationDetailsGroupBox", true, userControl.ConfigurationDetailsGroupBox.Controls.Contains(iMDefaultIncoTermBoundDropEdit));
			});
		}

		public void TestGenerateBackOrdersOnShortfallsCheckBox()
		{
			var generateBackOrdersOnShortfallsCheckBox = userControl.GenerateBackOrdersOnShortfallsCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", generateBackOrdersOnShortfallsCheckBox);
				AssertEquals("BindTo", nameof(OrgHeader.MiscServ) + "." + nameof(OrgHeader.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls), generateBackOrdersOnShortfallsCheckBox.BindTo);
				AssertEquals("Within BackOrdersGroupBox", true, userControl.BackOrdersGroupBox.Controls.Contains(generateBackOrdersOnShortfallsCheckBox));
			});
		}

		public void TestWhsPackingSlipOrderByDropEdit()
		{
			var whsPackingSlipOrderByDropEdit = userControl.WhsPackingSlipOrderByDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", whsPackingSlipOrderByDropEdit);
				AssertEquals("BindTo", nameof(OrgHeader.MiscServ) + "+" + nameof(OrgHeader.MiscServ.OM_WhsPackingSlipOrderBy), whsPackingSlipOrderByDropEdit.BindTo);
				AssertEquals("Within PackingSlipOrderByGroupBox", true, userControl.PackingSlipOrderByGroupBox.Controls.Contains(whsPackingSlipOrderByDropEdit));
			});
		}

		public void TestRecalculateOrderPricingCheckBox()
		{
			var recalculateOrderPricingCheckBox = userControl.RecalculateOrderPricingCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", recalculateOrderPricingCheckBox);
				AssertEquals("BindTo", nameof(OrgHeader.MiscServ) + "." + nameof(OrgHeader.MiscServ.OM_WhsIsRecalculateOrderPricing), recalculateOrderPricingCheckBox.BindTo);
				AssertEquals("Within RecalculateOrderPricingGroupBox", true, userControl.RecalculateOrderPricingGroupBox.Controls.Contains(recalculateOrderPricingCheckBox));
			});
		}

		public void TestCusInventoryForOutwardProcessingCheckBox()
		{
			var cusInventoryForOutwardProcessingCheckBox = userControl.CusInventoryForOutwardProcessingCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", cusInventoryForOutwardProcessingCheckBox);
				AssertEquals("BindTo", nameof(OrgHeader.CompanyData) + "+" + nameof(OrgHeader.CompanyData.OB_CusInventoryForOutwardProcessing), cusInventoryForOutwardProcessingCheckBox.BindTo);
				AssertEquals("Within BondedWarehouseAutomationGroupBox", true, userControl.BondedWarehouseAutomationGroupBox.Controls.Contains(cusInventoryForOutwardProcessingCheckBox));
			});
		}

		public void TestCusInventoryForInwardProcessingCheckBox()
		{
			var cusInventoryForInwardProcessingCheckBox = userControl.CusInventoryForInwardProcessingCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", cusInventoryForInwardProcessingCheckBox);
				AssertEquals("BindTo", nameof(OrgHeader.CompanyData) + "+" + nameof(OrgHeader.CompanyData.OB_CusInventoryForInwardProcessing), cusInventoryForInwardProcessingCheckBox.BindTo);
				AssertEquals("Within BondedWarehouseAutomationGroupBox", true, userControl.BondedWarehouseAutomationGroupBox.Controls.Contains(cusInventoryForInwardProcessingCheckBox));
			});
		}

		public void TestCusInventoryForVatWarehouseCheckBox()
		{
			var iMUsedVatWarehouseCheckBox = userControl.IMUsedVatWarehouseCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", iMUsedVatWarehouseCheckBox);
				AssertEquals("BindTo", nameof(OrgHeader.CompanyData) + "+" + nameof(OrgHeader.CompanyData.OB_IMUsedVatWarehouse), iMUsedVatWarehouseCheckBox.BindTo);
				AssertEquals("Within BondedWarehouseAutomationGroupBox", true, userControl.BondedWarehouseAutomationGroupBox.Controls.Contains(iMUsedVatWarehouseCheckBox));
			});
		}

		public void TestBondedWarehouseAutomationGroupBox()
		{
			var bondedWarehouseAutomationGroupBox = userControl.BondedWarehouseAutomationGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Inventory Management", bondedWarehouseAutomationGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.None, bondedWarehouseAutomationGroupBox.Dock);
			});
		}

		public void TestDGContactDetailsGroupBox()
		{
			var dGContactDetailsGroupBox = userControl.DGContactDetailsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "DG Contact Details", dGContactDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.None, dGContactDetailsGroupBox.Dock);
			});
		}

		public void TestConfigurationDetailsGroupBox()
		{
			var configurationDetailsGroupBox = userControl.ConfigurationDetailsGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Importer / Consignee Configuration", configurationDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.None, configurationDetailsGroupBox.Dock);
			});
		}

		public void TestBackOrdersGroupBox()
		{
			var backOrdersGroupBox = userControl.BackOrdersGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Back Orders", backOrdersGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.None, backOrdersGroupBox.Dock);
			});
		}

		public void TestPackingSlipOrderByGroupBox()
		{
			var packingSlipOrderByGroupBox = userControl.PackingSlipOrderByGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Order Copy/Packing Slip Order By", packingSlipOrderByGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.None, packingSlipOrderByGroupBox.Dock);
			});
		}

		public void TestRecalculateOrderPricingGroupBox()
		{
			var recalculateOrderPricingGroupBox = userControl.RecalculateOrderPricingGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Order pricing", recalculateOrderPricingGroupBox.CaptionResourceString.Caption);
				AssertEquals("Dock", System.Windows.Forms.DockStyle.None, recalculateOrderPricingGroupBox.Dock);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new WhsDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		WhsDetailsUserControl userControl;
	}
}
