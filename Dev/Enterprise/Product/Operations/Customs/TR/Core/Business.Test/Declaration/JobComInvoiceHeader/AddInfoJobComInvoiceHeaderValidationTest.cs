using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_CommercialPaymentCode()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoice.ZG_CommercialPaymentCodeInfo, "99", "1");
		}
	}
}
