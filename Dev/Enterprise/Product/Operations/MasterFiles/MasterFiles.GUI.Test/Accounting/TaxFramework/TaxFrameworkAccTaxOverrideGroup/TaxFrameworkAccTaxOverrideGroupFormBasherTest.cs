using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TaxFrameworkAccTaxOverrideGroupForm))]
	sealed class TaxFrameworkAccTaxOverrideGroupFormBasherTest : AccTaxOverrideGroupFormBaseBasherTest
	{
		public void TestFormCaption()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			using (var testForm = new TaxFrameworkAccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				AssertEquals("Form Caption should be 'Tax Configuration Override Group'", "Tax Configuration Override Group", testForm.FormCaption);
			}
		}

		[RequiresSTA]
		public void TestFormSetsTaxFrameworkContext()
		{
			AccTaxOverrideGroup taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			AssertEquals(false, Factory.HasContext(AccTaxOverrideGroup.BusinessContext.TaxFramework));

			using (var testForm = new TaxFrameworkAccTaxOverrideGroupForm(taxOverrideGroup))
			{
				AssertEquals(true, Factory.HasContext(AccTaxOverrideGroup.BusinessContext.TaxFramework));
			}
		}

		public void TestTaxFrameworkConfigurationTab()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (var testForm = new TaxFrameworkAccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				var tabControl = testForm.Controls["MainTabControl"] as ZTemplateTabControl;
				AssertNotNull("Precondition: tab control", tabControl);
				var tabPage = tabControl.TabPages["TaxFrameworkConfigurationTabPage"];
				CombineAssertions(() =>
				{
					AssertNotNull("Tax Framework Configuration tab must be not null", tabPage);
					AssertEquals("TabPage's Text", "Tax Framework Configuration", tabPage.Text);
					AssertEquals("Tax Framework Configuration tab will contain 1 control", 1, tabPage.Controls.Count);
					AssertNotNull(tabPage.Controls.Find("TaxFrameworkTaxConfigurationGrid", false));
				});
			}
		}

		[RequiresSTA]
		public void TestLinkedChargeCodesTab()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (var testForm = new TaxFrameworkAccTaxOverrideGroupForm(taxOverrideGroup))
			{
				testForm.Show();
				var tabControl = testForm.Controls["MainTabControl"] as ZTemplateTabControl;
				AssertNotNull("Precondition: tab control", tabControl);
				var tabPage = tabControl.TabPages["LinkedChargeCodesTabPage"];
				CombineAssertions(() =>
				{
					AssertNotNull("Linked Charge Codes tab must be not null", tabPage);
					AssertEquals("TabPage's Text", "Linked Charge Codes", tabPage.Text);
					AssertEquals("Linked Charge Codes tab will contain 1 control", 1, tabPage.Controls.Count);
					AssertNotNull(tabPage.Controls.Find("LinkedChargeCodesGroupBox", false));
				});
			}
		}

		public void TestTaxFrameworkTaxConfigurationsGrid()
		{
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			using (var form = new TaxFrameworkAccTaxOverrideGroupForm(taxOverrideGroup))
			{
				form.Show();
				var taxFrameworkTaxConfigurationGrid = (ZGrid)form.Controls["MainTabControl"].Controls["TaxFrameworkConfigurationTabPage"].Controls["TaxFrameworkTaxConfigurationGrid"];

				var expectedListOfColumns = new[]
				{
						$"{AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_ETC_TaxConfiguration} (ZGuidDropEditColumnStyleInfo) IsVisible:True",
						$"Ledger (ZTextBoxColumnStyleInfo) IsVisible:True",
						$"TaxConfigurationDescription (ZTextBoxColumnStyleInfo) IsVisible:True",
						$"TaxConfigurationBranch (ZTextBoxColumnStyleInfo) IsVisible:True",
						$"{AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_RateNumerator} (ZCalcEditColumnStyleInfo) IsVisible:True",
						$"{AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_RateDenominator} (ZCalcEditColumnStyleInfo) IsVisible:True",
						$"Rate (ZCalcEditColumnStyleInfo) IsVisible:True",
						$"{AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_TaxAuthorityServiceCode} (ZTextBoxColumnStyleInfo) IsVisible:True",
						$"{AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_TaxAuthorityServiceCodeDescription} (ZTextBoxColumnStyleInfo) IsVisible:True",
						$"{AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_AT_TaxID} (ZGuidFindBoxColumnStyleInfo) IsVisible:True",
						$"TaxRateSource (ZTextBoxColumnStyleInfo) IsVisible:True",
						$"{AccTaxOverrideGroupTaxConfigurationPivot.Schema.AXP_A9_DefaultVATClass} (ZGuidFindBoxColumnStyleInfo) IsVisible:True",
				};
				var realListOfColumns = taxFrameworkTaxConfigurationGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new TaxFrameworkAccTaxOverrideGroupForm(Factory.New<AccTaxOverrideGroup>());
		}

		#endregion
	}
}
