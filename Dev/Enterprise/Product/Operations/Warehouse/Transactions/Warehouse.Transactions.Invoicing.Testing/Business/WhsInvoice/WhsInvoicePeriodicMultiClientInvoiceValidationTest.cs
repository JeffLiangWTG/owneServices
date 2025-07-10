using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	public class WhsInvoicePeriodicMultiClientInvoiceValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestInvoiceDateValidation

		public void TestInvoiceDateValidation()
		{
			var whsInvoicePeriodicMultiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			whsInvoicePeriodicMultiClientInvoice.InvoiceDate = ZDate.Empty;
			AssertHasError(whsInvoicePeriodicMultiClientInvoice.InvoiceDateInfo, "Please enter a value.");

			whsInvoicePeriodicMultiClientInvoice.InvoiceDate = ZDate.Invalid;
			AssertHasError(whsInvoicePeriodicMultiClientInvoice.InvoiceDateInfo, "Enter a valid selection.");

			whsInvoicePeriodicMultiClientInvoice.InvoiceDate = ZDate.Today;
			AssertNoErrors(whsInvoicePeriodicMultiClientInvoice.InvoiceDateInfo);
		}

		#endregion
	}
}