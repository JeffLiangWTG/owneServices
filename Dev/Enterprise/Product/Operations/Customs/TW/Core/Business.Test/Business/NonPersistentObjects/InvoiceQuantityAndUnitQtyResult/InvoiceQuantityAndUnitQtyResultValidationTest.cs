using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class InvoiceQuantityAndUnitQtyResultValidationTest : BusinessObjectValidationTestCase
	{
		public void TestType()
		{
			var invoiceQuantityAndUnitQtyResult = new InvoiceQuantityAndUnitQtyResult();
			AssertType<InvoiceQuantityAndUnitQtyResultValidation>(invoiceQuantityAndUnitQtyResult.Validation);
		}
	}
}
