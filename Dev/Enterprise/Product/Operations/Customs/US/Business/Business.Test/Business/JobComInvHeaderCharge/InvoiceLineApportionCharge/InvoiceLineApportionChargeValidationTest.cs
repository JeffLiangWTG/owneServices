namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceLineApportionChargeValidationTest : Customs.Business.Testing.JobComInvHeaderChargeValidationTest
	{
		public void TestCheckJ7_FullOrPartialApportionment()
		{
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			InvoiceLineApportionCharge apportCharge = line.ApportionedCharges.AddNew();
			apportCharge.J7_FullOrPartialApportionment = "~";
			AssertHasMessageError(apportCharge.J7_FullOrPartialApportionmentInfo, InvoiceLineApportionChargeValidation.FullOrPartialApportionmentShouldBeInList);
			apportCharge.J7_FullOrPartialApportionment = apportCharge.Lookups.ApportionmentTypeList[0].Code;
			AssertNoMessageError(apportCharge.J7_FullOrPartialApportionmentInfo, InvoiceLineApportionChargeValidation.FullOrPartialApportionmentShouldBeInList);
		}

		public void TestIsCIFComponentUsed()
		{
			Assert(new InvoiceLineApportionChargeValidationForTest(Factory.New<InvoiceLineApportionCharge>()).IsCIFComponentUsedExposed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;

		sealed class InvoiceLineApportionChargeValidationForTest : InvoiceLineApportionChargeValidation
		{
			public InvoiceLineApportionChargeValidationForTest(InvoiceLineApportionCharge invoiceLineCharge) : base(invoiceLineCharge)
			{
			}

			internal bool IsCIFComponentUsedExposed => IsCIFComponentUsed;
		}
	}
}
