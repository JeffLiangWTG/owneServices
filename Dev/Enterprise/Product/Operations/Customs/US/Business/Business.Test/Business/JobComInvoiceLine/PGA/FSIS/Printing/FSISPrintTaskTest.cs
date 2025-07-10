using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class FSISPrintTaskTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var fsisLine = invoiceLine.FSISLines.AddNew();
			declaration.FSISLinesForPrint = new USInvoiceLineFSISLine[] { fsisLine };
			FSISPrintTask task = new FSISPrintTask(declaration);
			Assert(task.IsTaskCreated);
		}
	}
}
