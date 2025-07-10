using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceHeaderCloneStrategyTest : TestCaseWithFactory
	{
		public void TestValidationDateIsCleared()
		{
			JobDeclaration testDec = (JobDeclaration)JobDeclaration.New(Factory);
			testDec.JE_MasterBill = "MB123";
			JobComInvoiceGroupHeader group = testDec.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "SG-Inv111~~~";
			invoice.JZ_InvoiceCurrExRate = 1.375m;
			invoice.JZ_ValuationDateOverride = new ZDateTime(2009, 07, 12);
			invoice.Charges.AddNew();
			invoice.GroupCharges.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			JobDeclaration clonedDec = (JobDeclaration)testDec.TemplateCopy();
			JobComInvoiceHeader clonedInvoice = clonedDec.Invoices[0];
			AssertEquals("ClonedDec valuation date is empty", true, clonedInvoice.JZ_ValuationDateOverride.IsEmpty);
		}
	}
}
