using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.DIS.Testing
{
	sealed class InvoiceValueProviderTest : TestCaseWithFactory
	{
		public void TestDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "ABC3428";
			invoice.JZ_InvoiceAmount = 12500m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var invoices = new InvoiceValueProvider(declaration).Invoices;
			AssertEquals(1, invoices.Count());

			var disInvoice = invoices.ElementAt(0);
			AssertEquals(2, disInvoice.InvoiceLines.Count());
			AssertEquals("ABC3428", disInvoice.InvoiceNumber);

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ABCDMAN";
			invoice.JZ_OH_Supplier = supplier.PK;

			AssertEquals("Supplier: ABCDMAN, Amount: 12500.00 USD", disInvoice.Description);
		}
	}
}
