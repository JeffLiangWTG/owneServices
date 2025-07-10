using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SPILineTest : TestCaseWithFactory
	{
		public void TestConstructorWithInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var spiLine = SPILine.New(invoiceLine);
			AssertEquals(typeof(SPINormalInvoiceLine), spiLine.GetType());
			invoiceLine.US_SupTariff = "9802";
			spiLine = SPILine.New(invoiceLine);
			AssertEquals(typeof(SPISupInvoiceLine), spiLine.GetType());
		}
	}
}
