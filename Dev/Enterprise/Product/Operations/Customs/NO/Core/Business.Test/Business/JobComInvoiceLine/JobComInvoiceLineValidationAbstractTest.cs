using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	abstract class JobComInvoiceLineValidationAbstractTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;

		protected abstract string MessageType { get; }
	}
}
