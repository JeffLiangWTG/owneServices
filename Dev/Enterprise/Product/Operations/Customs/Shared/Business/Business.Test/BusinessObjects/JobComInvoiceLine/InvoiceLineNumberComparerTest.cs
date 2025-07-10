using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class InvoiceLineNumberComparerTest : TestCaseWithFactory
	{
		public void TestInvoiceLineNumberComparer()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;

			var comparer = new InvoiceLineNumberComparer();
			AssertEquals("Negative", true, comparer.Compare(invoiceLine, invoiceLine2) < 0);

			invoiceLine2.JI_LineNo = 1;
			invoiceLine.JI_LineNo = 2;
			AssertEquals("Positive", true, comparer.Compare(invoiceLine, invoiceLine2) > 0);
		}
	}
}
