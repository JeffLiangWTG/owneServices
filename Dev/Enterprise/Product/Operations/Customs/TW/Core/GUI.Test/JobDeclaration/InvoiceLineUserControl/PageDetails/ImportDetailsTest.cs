using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class ImportDetailsTest : TestCaseWithFactory
	{
		public void TestSetControlsVisibilityForRAP()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.DutiesTaxesAndFeesTabPage;
					var importDetails = invoiceLineUserControl.FindSingleOrDefault<ImportDetailsUserControl>(c => c.Name == "ImportDetailsUserControl");
					line.JI_Procedure = Constants.ProcedureCodes._37;
					importDetails.SetControlsVisibilityForRAPROR(line);
					AssertEquals(true, importDetails.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "TW_RAPRORPriceConvertToLocalCurrencyControl").Visible);
					AssertEquals(true, importDetails.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl").Visible);
					AssertEquals(true, importDetails.FindSingleOrDefault<ZCheckBox>(c => c.Name == "JI_UseOneTenthCVCheckBox").Visible);
					AssertEquals(true, importDetails.FindSingleOrDefault<ZGroupBox>(c => c.Name == "RAPRORGroupBox").Visible);
					line.JI_Procedure = Constants.ProcedureCodes._04;
					importDetails.SetControlsVisibilityForRAPROR(line);
					AssertEquals(false, importDetails.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "TW_RAPRORPriceConvertToLocalCurrencyControl").Visible);
					AssertEquals(false, importDetails.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl").Visible);
					AssertEquals(false, importDetails.FindSingleOrDefault<ZCheckBox>(c => c.Name == "JI_UseOneTenthCVCheckBox").Visible);
					AssertEquals(false, importDetails.FindSingleOrDefault<ZGroupBox>(c => c.Name == "RAPRORGroupBox").Visible);
					line.JI_Procedure = Constants.ProcedureCodes._38;
					importDetails.SetControlsVisibilityForRAPROR(line);
					AssertEquals(true, importDetails.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "TW_RAPRORPriceConvertToLocalCurrencyControl").Visible);
					AssertEquals(true, importDetails.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl").Visible);
					AssertEquals(true, importDetails.FindSingleOrDefault<ZGroupBox>(c => c.Name == "RAPRORGroupBox").Visible);
				}
			}
		}

		public void TestImportDetails()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "EXP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.DutiesTaxesAndFeesTabPage;
					var importDetails = invoiceLineUserControl.FindSingleOrDefault<ImportDetailsUserControl>(c => c.Name == "ImportDetailsUserControl");
					AssertNull(importDetails);
				}
			}

			jobDeclartion.JE_MessageType = "IMP";
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.DutiesTaxesAndFeesTabPage;
					var importDetails = invoiceLineUserControl.FindSingleOrDefault<ImportDetailsUserControl>(c => c.Name == "ImportDetailsUserControl");
					AssertNotNull(importDetails);
				}
			}
		}

		public void TestSpecialDutyRatesGroupBoxCaption()
		{
			using (var control = new ImportDetailsUserControl())
			{
				var specialDutyRatesGroupBox = control.FindSingleOrDefault<ZGroupBox>(c => c.Name == "DutyRatesGroupBox");
				AssertEquals("Duties", specialDutyRatesGroupBox.CaptionResourceString.Caption);
			}
		}
	}
}
