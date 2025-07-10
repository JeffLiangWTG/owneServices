using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	public class WhsInvoicingTestCaseWithFactory : WhsTestCaseWithFactory
	{
		protected new WhsTestHelperFunctionsInvoice Helper => helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory));
		WhsTestHelperFunctionsInvoice helper;
	}
}