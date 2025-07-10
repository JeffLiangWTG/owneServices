using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceGroupHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestInvoiceGroupHeader()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Validation.InvoiceGroupHeader, groupHeader);
		}
	}
}
