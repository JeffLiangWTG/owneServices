using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupFormBase))]
	class AccTaxOverrideGroupFormBaseBasherTest : ZFormBasherTest
	{
		public void TestDebtorRoleColumnExists()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (var testForm = new AccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_DebtorRole", GetTaxOverridesGrid(testForm), true);
			}
		}

		public void TestShowAO_CustomsStatusColumnOnlyFroEUContries()
		{
			AccTaxOverrideGroupFormBase testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_CustomsStatus", GetTaxOverridesGrid(testForm), false);
			}

			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;

				using (testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
				{
					testForm.Show();
					AssertColumnExist("AO_CustomsStatus", GetTaxOverridesGrid(testForm), true);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		public void TestShowAO_SplitPaymentVATOrganisationColumnOnlyFroItaly()
		{
			AccTaxOverrideGroupFormBase testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_SplitPaymentVATOrganisation", GetTaxOverridesGrid(testForm), false);
			}

			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				using (testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
				{
					testForm.Show();
					AssertColumnExist("AO_SplitPaymentVATOrganisation", GetTaxOverridesGrid(testForm), false);
				}

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
				using (testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
				{
					testForm.Show();
					AssertColumnExist("AO_SplitPaymentVATOrganisation", GetTaxOverridesGrid(testForm), true);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		[RequiresSTA]
		public void TestShowAO_GBColumn()
		{
			AccTaxOverrideGroupFormBase testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_GB", GetTaxOverridesGrid(testForm), false);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_GB", GetTaxOverridesGrid(testForm), true);
			}
		}

		[RequiresSTA]
		public void TestShowAO_CreateTaxRecordColumn()
		{
			AccTaxOverrideGroupFormBase testForm;
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
			{
				testForm.Show();
				AssertColumnExist("AO_CreateTaxRecord", GetTaxOverridesGrid(testForm), true);
			}
		}

		public void TestTaxOverridesGridShowColumnAO_SupplyType()
		{
			AssertByRegistry(true);
			AssertByRegistry(false);

			void AssertByRegistry(bool enableRegisrty)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableRegisrty))
				using (var testForm = new AccTaxOverrideGroupForm(Factory.NewWithValidTestData<AccTaxOverrideGroup>()))
				{
					testForm.Show();
					AssertColumnExist("AO_SupplyType", GetTaxOverridesGrid(testForm), enableRegisrty);
				}
			}
		}

		public virtual void TestTaxOverridesGroupBoxText()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (var testForm = new AccTaxOverrideGroupFormBase(taxOverrideGroup))
			{
				testForm.Show();
				Application.DoEvents();
				var taxOverridesGroupBox = testForm.GetControl<ZGroupBox>("TaxOverridesGroupBox");
				AssertEquals("Tax Overrides", taxOverridesGroupBox.Text);
			}
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (AccTaxOverrideGroupFormBase)GetFormToBashCore())
			{
				AssertNotNull("Tax Override Group form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new AccTaxOverrideGroupFormBase(Factory.New<AccTaxOverrideGroup>());
		}

		protected void AssertColumnExist(string columnName, ZGrid grid, bool mustExist)
		{
			bool theColumnExist = false;
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				if (column.ColumnName == columnName)
				{
					theColumnExist = true;
					break;
				}
			}
			AssertEquals(string.Format("Column {0} must{1} exits in grid {2}", columnName, mustExist ? "" : " not", grid.Name), mustExist, theColumnExist);
		}

		protected ZGrid GetTaxOverridesGrid(AccTaxOverrideGroupFormBase form)
		{
			ZTemplateTabControl tabControl = form.Controls["MainTabControl"] as ZTemplateTabControl;
			AssertNotNull("Precondition: tab control", tabControl);
			TabPage tabPage = tabControl.TabPages["TaxOverrideGroupsTabPage"];
			AssertNotNull("Precondition: tab page", tabPage);
			Control control = tabPage.Controls["TaxOverridesGroupBox"];
			AssertNotNull("Precondition: group box", control);

			ZGrid grid = control.Controls["TaxOverridesGrid"] as ZGrid;
			AssertNotNull("Precondition:", grid);
			return grid;
		}

		#endregion
	}
}
