using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ImportJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestValidationType()
		{
			AssertType<ImportJobComInvoiceLineValidation>(invoiceLine.Validation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
