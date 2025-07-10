using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ReconInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestSomeOverrideColumnsAndTaxRateColumns()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.Invoice.InvoiceLines.AddNew();
			using (var form = new ReconDeclarationForm(reconDeclaration))
			{
				form.Show();
				var grid = form.EntryLinesReconInvoiceLineUserControl.InvoicesGrid;
				AssertNotNull(grid.GetColumnStyle("US_R_OverrideOriginHMF"));
				AssertNotNull(grid.GetColumnStyle("US_R_OverrideReconHMF"));
				AssertNotNull(grid.GetColumnStyle("US_R_OverrideOrigOtherFeeAmount"));
				AssertNotNull(grid.GetColumnStyle("US_R_OverrideReconOtherFeeAmount"));
				AssertNotNull(grid.GetColumnStyle("US_R_OrigTaxRate"));
				AssertNotNull(grid.GetColumnStyle("US_TaxRate"));
				AssertNotNull(grid.GetColumnStyle("US_R_OrigTaxQty"));
				AssertNotNull(grid.GetColumnStyle("US_TaxQty"));
			}
		}

		public void TestUS_R_OrigEntryLineNoColumnReadOnly()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.OriginalEntries.AddNew();
			var invoice = reconDeclaration.Invoices.AddNew();
			invoice.US_CH_ReconEntry = reconDeclaration.OriginalEntries[0].CH_PK;
			reconDeclaration.InvoiceLines.AddNew();
			using (var form = new ReconDeclarationForm(reconDeclaration))
			{
				form.Show();
				var userControl = form.EntryLinesReconInvoiceLineUserControl;
				var columnStyle = userControl.InvoicesGrid.GetColumnStyle("US_R_OrigEntryLineNo");
				AssertEquals("Should not be readonly", false, columnStyle.IsReadOnly);
			}
		}

		public void TestMPFColumns()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.Invoice.InvoiceLines.AddNew();
			using (var form = new ReconDeclarationForm(reconDeclaration))
			{
				form.Show();
				var userControl = form.EntryLinesReconInvoiceLineUserControl;
				var reconMPFAmountColumnStyle = userControl.InvoicesGrid.GetColumnStyle("US_R_ReconMPFAmount");
				AssertNotNull(reconMPFAmountColumnStyle);
				var overrideOriginMPFColumnStyle = userControl.InvoicesGrid.GetColumnStyle("US_R_OverrideOriginMPF");
				AssertNotNull(overrideOriginMPFColumnStyle);
				var overrideReconMPFColumnStyle = userControl.InvoicesGrid.GetColumnStyle("US_R_OverrideReconMPF");
				AssertNotNull(overrideReconMPFColumnStyle);
			}
		}

		public void TestReconOtherFeeColumns()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var originalEntry = reconDeclaration.OriginalEntries.AddNew();
			originalEntry.Invoice.InvoiceLines.AddNew();
			using (var form = new ReconDeclarationForm(reconDeclaration))
			{
				form.Show();
				var userControl = form.EntryLinesReconInvoiceLineUserControl;
				var reconOtherFeeCodeColumnStyle = userControl.InvoicesGrid.GetColumnStyle("US_R_ReconOtherFeeCode");
				AssertNotNull(reconOtherFeeCodeColumnStyle);
				var reconOtherFeeAmountColumnStyle = userControl.InvoicesGrid.GetColumnStyle("US_R_ReconOtherFeeAmount");
				AssertNotNull(reconOtherFeeAmountColumnStyle);
			}
		}
	}
}
