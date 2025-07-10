using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobComInvoiceGroupHeaderDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestClone()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.AutoCreateChargesBasedOnIncoTerm = false;
			declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew("OFT", 10m, "AUD");
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();

			BaseJobDeclaration clonedDec = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(1, clonedDec.JobComInvoiceGroupHeaders.Count);
			AssertEquals(2, clonedDec.Invoices.Count);
			AssertEquals(2, clonedDec.InvoiceLines.Count);

			AssertEquals(1, clonedDec.JobComInvoiceGroupHeaders[0].Charges.Count);
			AssertEquals(1, clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals(1, clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);

			AssertEquals(1, clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.Count);
			AssertEquals(1, clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals(1, clonedDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);
		}
	}
}
