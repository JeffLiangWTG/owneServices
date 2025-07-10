using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceGroupHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestInvoiceGroupHeader()
		{
			JobComInvoiceGroupHeader groupHeader = Factory.New<JobComInvoiceGroupHeader>();
			AssertEquals(groupHeader.Validation.InvoiceGroupHeader, groupHeader);
		}
	}
}
