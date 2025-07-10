using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	sealed class USInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestLineSummaryPanel()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				USInvoiceLineUserControl userControl = (USInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LineChargesTabPage;
				AssertEquals("Charges grid GST Applicable column text", USCustomsSupplierHeaderUserControl.IsCIFComponent, userControl.InvoiceLineCharges.ChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
				AssertEquals("Charges grid GST Applicable column text", USCustomsSupplierHeaderUserControl.IsCIFComponent, userControl.InvoiceLineCharges.ApportionedChargesGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
				AssertNotNull(userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_FormattedTariff]);
				AssertNotNull(userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff));
				AssertEquals(false, userControl.JI_Calc_GSTConvertToLocalCurrencyControl.Visible);
				AssertEquals(false, userControl.FindSingle<ConvertToLocalCurrencyControl>("JI_Calc_CIFConvertToLocalCurrencyControl").Visible);
			}
		}
	}
}
