using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceLineEnteredValueForOGATest : TestCaseWithFactory
	{
		public void TestGetEnteredValueForOGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500.51m;

			AssertEquals(501m, invoiceLine.GetEnteredValueForOGA());

			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(501m, invoiceLine.GetEnteredValueForOGA());
		}
	}
}
