using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Drawback7551DocLine))]
	class Drawback7551DocLineTest : DrawbackExportDocLineTest
	{
		public void TestShouldShowSubTotals()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPrintSubTotals = false;
			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			drawback.US_ClaimPort = "2809";
			var drawback7551DocLine = new Drawback7551DocLine(invoiceLine);
			Assert(!drawback7551DocLine.ShouldShowSubTotals);

			drawback.US_ClaimPort = "3901";
			drawback7551DocLine = new Drawback7551DocLine(invoiceLine);
			Assert(!drawback7551DocLine.ShouldShowSubTotals);

			drawback.US_DRWPrintSubTotals = true;
			drawback7551DocLine = new Drawback7551DocLine(invoiceLine);
			Assert(drawback7551DocLine.ShouldShowSubTotals);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			return new Drawback7551DocLine(invoiceLine);
		}

		protected override DrawbackDocLine GetNewDrawbackExportDocLine(JobComInvoiceLine invoiceLine)
		{
			return new Drawback7551DocLine(invoiceLine);
		}
	}
}
