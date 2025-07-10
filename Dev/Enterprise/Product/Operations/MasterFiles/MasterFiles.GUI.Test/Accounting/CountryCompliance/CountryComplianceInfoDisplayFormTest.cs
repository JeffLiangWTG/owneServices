using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Accounting.CountryCompliance.Testing
{
	[TestedType(typeof(CountryComplianceInfoDisplayForm))]
	sealed class CountryComplianceInfoDisplayFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new CountryComplianceInfoDisplayForm(new CountryComplianceInfoDisplay());
			form.ControllerID = ControllerIDs.AccTaxRate;
			return form;
		}

		[RequiresSTA]
		public void TestComplianceSubtypeInfoFieldsAndColumnsDisplay()
		{
			var testObject = new CountryComplianceInfoDisplay();

			using (var form = new CountryComplianceInfoDisplayForm(testObject))
			{
				form.Show();

				var grid = form.Controls.Find("complianceSubTypesDefaultingRulesGrid", true)[0] as ZGrid;
				AssertNotNull(grid);

				AssertGridColumnInfo(grid, "Country", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "SubType", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "Description", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "DocumentTitle", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "LedgerType", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "InvoiceType", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "TaxInvoiceRule", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "OriginalRule", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "DisbursementRule", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "OrganisationLocation", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "TaxRegistrationType", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "SelfBillingRule", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "TaxRegistrationLocationRule", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "VATGroupRule", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "RuleSetCode", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "RuleSetDescription", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "ParentTransactionSubType", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "TaxIDCode", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "ExporterExemption", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "RequiredTaxSystem", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "ExcludedTaxSystem", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "RequiredRegistrationCode", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "ExcludedRegistrationCode", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "ThresholdApplies", typeof(ZCheckBoxColumnStyle));
				AssertGridColumnInfo(grid, "SubTypeThresholdNotMet", typeof(ZTextBoxColumnStyle));
			}
		}

		public void TestDefaultRegistryValueTypeInfoFieldsAndColumnsDisplay()
		{
			var testObject = new CountryComplianceInfoDisplay();

			using (var form = new CountryComplianceInfoDisplayForm(testObject))
			{
				form.Show();

				var grid = form.GetControl<ZGrid>("defaultRegistryValueGrid");

				AssertGridColumnInfo(grid, "Caption", typeof(ZTextBoxColumnStyle));
				AssertGridColumnInfo(grid, "DefaultValue", typeof(ZTextBoxColumnStyle));
			}
		}

		void AssertGridColumnInfo(ZGrid grid, ZString columnName, Type expectedStyleType)
		{
			var columnInfo = grid.GetColumnStyle(columnName);

			AssertNotNull(columnName, columnInfo);
			AssertEquals($"{columnName} Style Type", expectedStyleType, columnInfo.ColumnStyleType);
			Assert($"{columnName} IsVisible", columnInfo.IsVisible);
		}
	}
}
