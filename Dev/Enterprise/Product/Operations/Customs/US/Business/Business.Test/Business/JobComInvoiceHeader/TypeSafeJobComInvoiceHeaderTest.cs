using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TypeSafeJobComInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestGetNewValidation()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(JobComInvoiceHeaderValidation), invoice.Validation.GetType());
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);
			AssertEquals(typeof(EmptyInvoiceHeaderValidation), invoice.Validation.GetType());
		}

		public void TestIncotermFactory()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(DeliveryTermAndChargeFactory), invoice.IncoTermAndChargeFactory.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(Common.CommonIncoTermAndCustomsChargeFactory), invoice.IncoTermAndChargeFactory.GetType());
		}

		public void TestInvoiceLinesIsAnEditableChildOfInvoiceHeaderForRecon()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = entry.Invoice;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			invoiceLine.JI_LinePrice = 1200m; //should cause recalculation of customs fees for 'entry' for its total fee.
			AssertEquals(true, entry.HasChanges);
		}

		public void TestInvoiceChargeCollection()
		{
			AssertEquals(typeof(InvoiceChargeCollection), invoice.Charges.GetType());
		}

		public void TestApportionChargeCollection()
		{
			AssertEquals(typeof(InvoiceApportionChargeCollection), invoice.GroupCharges.GetType());
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
		}
	}
}
