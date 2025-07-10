using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupForm))]
	class AccTaxOverrideGroupFormBasherTest : AccTaxOverrideGroupFormBaseBasherTest
	{
		[RequiresSTA]
		public void TestColumnsAvailableAreSameAsChargeCodeFormGrid()
		{
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));
			ZGrid chargeCodeFormGrid;

			AssertNotNull("Precondition", chargeCode);
			using (var testForm = new AccChargeCodeForm(chargeCode))
			{
				testForm.Show();

				var testTabControl = (ZTemplateTabControl)testForm.Controls["ChargeCodeTabControl"];
				AssertNotNull("Precondition:", testTabControl);
				testTabControl.SelectTab("TaxOverridesTabPage");
				Application.DoEvents();

				chargeCodeFormGrid = (ZGrid)testTabControl.SelectedTab.Controls["TaxOverridesGrid"];
				AssertNotNull("Precondition:", chargeCodeFormGrid);
			}

			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			ZGrid taxOverrideGroupFormGrid;

			AssertNotNull("Precondition", taxOverrideGroup);
			using (var testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();

				taxOverrideGroupFormGrid = GetTaxOverridesGridForLinkedChargeCodes(testForm);
				AssertNotNull("Precondition:", taxOverrideGroupFormGrid);
			}

			AssertBothGridHasSameColumns(chargeCodeFormGrid, taxOverrideGroupFormGrid);
		}

		public void TestExporterExemptionAndHomeCountryColumnExists()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (var testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_VATExemptOnExportCharges", GetTaxOverridesGridForLinkedChargeCodes(testForm), true);
				AssertColumnExist("AO_HomeCountryOrZone", GetTaxOverridesGridForLinkedChargeCodes(testForm), true);
			}
		}

		public void TestDebtorRoleColumnExistsForLinkedChargeCodes()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (var testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_DebtorRole", GetTaxOverridesGridForLinkedChargeCodes(testForm), true);
			}
		}

		[RequiresSTA]
		public void TestDeleteDuplicateOverridesButton()
		{
			AccTaxOverrideGroupForm testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			AccChargeTaxOverride taxOverrideForGroup = taxOverrideGroup.TaxOverrides.AddNew();
			taxOverrideForGroup.AO_Direction = "ALL";
			taxOverrideForGroup.AO_IncoTerm = "ALL";
			taxOverrideForGroup.AO_JobType = "ALL";
			taxOverrideForGroup.AO_Origin = "ALL";
			taxOverrideForGroup.AO_Destination = "ALL";
			taxOverrideForGroup.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForGroup.AO_CustomsStatus = "ALL";

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			AccChargeTaxOverride taxOverrideForChargeCode1 = chargeCode.TaxOverrides.AddNew();
			taxOverrideForChargeCode1.AO_Direction = "ALL";
			taxOverrideForChargeCode1.AO_IncoTerm = "ALL";
			taxOverrideForChargeCode1.AO_JobType = "ALL";
			taxOverrideForChargeCode1.AO_Origin = "ALL";
			taxOverrideForChargeCode1.AO_Destination = "ALL";
			taxOverrideForChargeCode1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForChargeCode1.AO_CustomsStatus = "ALL";
			AccChargeTaxOverride taxOverrideForChargeCode2 = chargeCode.TaxOverrides.AddNew();
			taxOverrideForChargeCode2.AO_Direction = "ALL";
			taxOverrideForChargeCode2.AO_IncoTerm = "ALL";
			taxOverrideForChargeCode2.AO_JobType = "CST";
			taxOverrideForChargeCode2.AO_Origin = "ALL";
			taxOverrideForChargeCode2.AO_Destination = "ALL";
			taxOverrideForChargeCode2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverrideForChargeCode2.AO_CustomsStatus = "ALL";
			AssertEquals("Precondition: taxOverrideGroup.TaxOverrides.Count", 1, taxOverrideGroup.TaxOverrides.Count);
			AssertEquals("Precondition: chargeCode.TaxOverrides.Count", 2, chargeCode.TaxOverrides.Count);

			using (testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();

				ZTemplateTabControl tabControl = testForm.Controls["MainTabControl"] as ZTemplateTabControl;
				AssertNotNull("Precondition: tab control", tabControl);
				TabPage tabPage = tabControl.TabPages["LinkedChargeCodesTabPage"];
				AssertNotNull("Precondition: tab page", tabPage);
				SplitContainer splitContainer = (SplitContainer)tabPage.Controls["ChargeCodesSplitContainer"];
				AssertNotNull("Precondition: split container", splitContainer);
				Control control = splitContainer.Panel1.Controls["ChargeCodesGroupBox"];
				AssertNotNull("Precondition: group box", control);
				Control grid = control.Controls["ChargeCodesModuleButtonGrid"];
				AssertNotNull("Precondition: grid", grid);

				ZButton deleteDuplicatesbutton = (ZButton)control.Controls["DeleteDuplicateOverridesButton"];
				AssertNotNull("Precondition:", deleteDuplicatesbutton);
				tabControl.SelectTab(tabPage);
				deleteDuplicatesbutton.PerformClick();

				AssertEquals("taxOverrideGroup.TaxOverrides.Count", 1, taxOverrideGroup.TaxOverrides.Count);
				AssertEquals("chargeCode.TaxOverrides.Count", 1, chargeCode.TaxOverrides.Count);
				AssertCollectionContains("chargeCode.TaxOverrides must contain not duplicated tax override.", taxOverrideForChargeCode2, chargeCode.TaxOverrides);
			}
		}

		public void TestDeleteDuplicateOverrideButton_PositionCheck()
		{
			AccTaxOverrideGroupForm testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			using (testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();

				Control control = testForm.GetControl<ZGroupBox>("ChargeCodesGroupBox");
				ZButton deleteDuplicateOverridesbutton = (ZButton)control.Controls["DeleteDuplicateOverridesButton"];

				var zindexofdeleteDuplicateOverridesbutton = control.Controls.GetChildIndex(deleteDuplicateOverridesbutton);
				AssertEquals("Delete duplicate override button should be visible on form", 0, zindexofdeleteDuplicateOverridesbutton);

				var bottomBoundDifference = control.Bottom - deleteDuplicateOverridesbutton.Bounds.Bottom;
				AssertCloseEnough("Bottom bound assert", 7, bottomBoundDifference, 5);

				var leftBoundDifference = deleteDuplicateOverridesbutton.Bounds.Left - control.Left;
				AssertCloseEnough("Left bound assert", 10, leftBoundDifference, 5);
			}
		}

		public void TestShowAO_SplitPaymentVATOrganisationColumnOnlyForItalyOnLinkedChargeCodesGrids()
		{
			AccTaxOverrideGroupForm testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_SplitPaymentVATOrganisation", GetTaxOverridesGridForLinkedChargeCodes(testForm), false);
			}

			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				using (testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
				{
					testForm.Show();
					AssertColumnExist("AO_SplitPaymentVATOrganisation", GetTaxOverridesGridForLinkedChargeCodes(testForm), false);
				}

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
				using (testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
				{
					testForm.Show();
					AssertColumnExist("AO_SplitPaymentVATOrganisation", GetTaxOverridesGridForLinkedChargeCodes(testForm), true);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		public void TestShowAO_CustomsStatusColumnOnlyFroEUContriesOnLinkedChargeCodesGrids()
		{
			AccTaxOverrideGroupForm testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_CustomsStatus", GetTaxOverridesGridForLinkedChargeCodes(testForm), false);
			}

			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;

				using (testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
				{
					testForm.Show();
					AssertColumnExist("AO_CustomsStatus", GetTaxOverridesGridForLinkedChargeCodes(testForm), true);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		public void TestChargeCodeTaxOverridesGridShowColumnAO_SupplyType()
		{
			AssertByRegistry(true);
			AssertByRegistry(false);

			void AssertByRegistry(bool enableRegisrty)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableRegisrty))
				using (var testForm = new AccTaxOverrideGroupForm(Factory.NewWithValidTestData<AccTaxOverrideGroup>()))
				{
					testForm.Show();
					AssertColumnExist("AO_SupplyType", GetTaxOverridesGridForLinkedChargeCodes(testForm), enableRegisrty);
				}
			}
		}

		[RequiresSTA]
		public void TestCreateTaxRecordColumnNotVisible()
		{
			AccTaxOverrideGroupForm testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_CreateTaxRecord", GetTaxOverridesGrid(testForm), false);
			}
		}

		public void TestFormCaption()
		{
			AssertFormCaption(Core.Constants.CountryCodes.Australia, "GST");
			AssertFormCaption(Core.Constants.CountryCodes.Argentina, "IVA");
			AssertFormCaption(Core.Constants.CountryCodes.Bangladesh, "VAT");

			void AssertFormCaption(string countryCode, string taxName)
			{
				AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
				{
					testForm.Show();
					AssertEquals($"Form Caption should contain '{taxName} Tax Override Group'", $"{taxName} Tax Override Group", testForm.FormCaption);
				}
			}
		}

		public override void TestTaxOverridesGroupBoxText()
		{
			AssertTaxOverridesGroupBoxText(Core.Constants.CountryCodes.Australia, "GST");
			AssertTaxOverridesGroupBoxText(Core.Constants.CountryCodes.Argentina, "IVA");
			AssertTaxOverridesGroupBoxText(Core.Constants.CountryCodes.Bangladesh, "VAT");

			void AssertTaxOverridesGroupBoxText(string countryCode, string taxName)
			{
				var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
				{
					testForm.Show();
					Application.DoEvents();
					var taxOverridesGroupBox = testForm.GetControl<ZGroupBox>("TaxOverridesGroupBox");
					AssertEquals($"{taxName} Tax Overrides", taxOverridesGroupBox.Text);
				}
			}
		}

		#region Implementation

		void AssertBothGridHasSameColumns(ZGrid chargeCodeFormGrid, ZGrid taxOverrideGroudFormGrid)
		{
			AssertEquals("Number of columns should be the same", chargeCodeFormGrid.ColumnStyles.Count, taxOverrideGroudFormGrid.ColumnStyles.Count);

			foreach (ZGridColumnInfo column in chargeCodeFormGrid.ColumnStyles)
			{
				AssertColumnExist(column.ColumnName, taxOverrideGroudFormGrid, true);
			}
		}

		ZGrid GetTaxOverridesGridForLinkedChargeCodes(AccTaxOverrideGroupForm form)
		{
			ZTemplateTabControl tabControl = form.Controls["MainTabControl"] as ZTemplateTabControl;
			AssertNotNull("Precondition: tab control", tabControl);
			TabPage tabPage = tabControl.TabPages["LinkedChargeCodesTabPage"];
			AssertNotNull("Precondition: tab page", tabPage);
			SplitContainer splitContainer = (SplitContainer)tabPage.Controls["ChargeCodesSplitContainer"];
			AssertNotNull("Precondition: split container", splitContainer);
			Control control = splitContainer.Panel2.Controls["ChargeCodeTaxOverridesGroupBox"];
			AssertNotNull("Precondition: group box", control);

			ZGrid grid = control.Controls["ChargeCodeTaxOverridesGrid"] as ZGrid;
			AssertNotNull("Precondition:", grid);
			return grid;
		}

		protected override Form GetFormToBashCore()
		{
			return new AccTaxOverrideGroupForm(Factory.New<AccTaxOverrideGroup>());
		}

		#endregion
	}
}
