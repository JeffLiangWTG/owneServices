using Enterprise.MasterFiles.Business.Organisation.Registry;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CreditControlledDocumentsCheckConfigurationInvoiceTypesTest : TestCase
	{
		public void TestInvoiceTypeList()
		{
			AssertEquals(3, CreditControlledDocumentsCheckConfigurationInvoiceTypes.InvoiceTypeList.Count);
			AssertEquals(true, CreditControlledDocumentsCheckConfigurationInvoiceTypes.InvoiceTypeList.ContainsCode("ALL"));
			AssertEquals(true, CreditControlledDocumentsCheckConfigurationInvoiceTypes.InvoiceTypeList.ContainsCode("DSB"));
			AssertEquals(true, CreditControlledDocumentsCheckConfigurationInvoiceTypes.InvoiceTypeList.ContainsCode("NDB"));
		}

		public void TestInvoiceTypes()
		{
			AssertEquals("Any Invoice Type", CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Description);
			AssertEquals("Any Disbursement Type", CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Description);
			AssertEquals("Not a Disbursement Type", CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Description);
		}
	}
}
