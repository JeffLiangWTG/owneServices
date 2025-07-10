using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWInvoiceLineDecimalPlacesCalculatorTest : TestCaseWithFactory
	{
		public void TestGetTWInvoiceLineMaxDecimalPlaces()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfoChild.TWL_DocumentaryQty = 2.254m;
			invoiceLine.AddInfoChild.TWL_DocumentaryUQ = "PK";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.AddInfoChild.TWL_DocumentaryQty = 2.2545422222m;
			invoiceLine2.AddInfoChild.TWL_DocumentaryUQ = "PK";

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.AddInfoChild.TWL_DocumentaryQty = 2m;
			invoiceLine3.AddInfoChild.TWL_DocumentaryUQ = "PK";
			var tWInvoiceLineDecimalPlacesCalculator = new TWInvoiceLineDecimalPlacesCalculator(invoiceHeader);
			AssertEquals(4, tWInvoiceLineDecimalPlacesCalculator.GetTWInvoiceLineMaxDecimalPlaces(JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name));

			invoiceLine2.AddInfoChild.TWL_DocumentaryQty = 2.255m;
			AssertEquals(3, tWInvoiceLineDecimalPlacesCalculator.GetTWInvoiceLineMaxDecimalPlaces(JobTWComInvoiceLineSchema.TWL_DocumentaryQty.Name));
		}
	}
}
