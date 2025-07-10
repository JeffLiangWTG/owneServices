using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ExportJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestValidationType()
		{
			AssertType<ExportJobComInvoiceLineValidation>(invoiceLine.Validation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
