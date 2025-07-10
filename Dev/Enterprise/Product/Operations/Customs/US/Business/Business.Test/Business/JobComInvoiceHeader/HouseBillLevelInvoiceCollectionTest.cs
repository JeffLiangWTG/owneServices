using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class HouseBillLevelInvoiceCollectionTest : Customs.Business.Testing.HouseBillLevelInvoiceCollectionTest
	{
		public void TestTotalInvoiceQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JobComInvoiceLines.AddNew().US_ManifestQty = 10;
			AssertEquals(10m, declaration.Invoices.TotalInvoiceQty);
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JobComInvoiceLines.AddNew().US_ManifestQty = 20;
			AssertEquals(30m, declaration.Invoices.TotalInvoiceQty);
		}
	}
}
