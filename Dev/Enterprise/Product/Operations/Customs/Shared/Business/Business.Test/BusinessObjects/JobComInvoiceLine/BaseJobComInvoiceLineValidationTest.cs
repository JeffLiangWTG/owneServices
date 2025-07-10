using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUnitPrice() => CombineAssertions(() =>
		{
			const string message = "is too large, the value's range of";
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.UnitPrice = 100m;
			AssertNoError(invoiceLine.UnitPriceInfo, message);

			invoiceLine.UnitPrice = 1000000000000000m;
			AssertHasErrorContaining(invoiceLine.UnitPriceInfo, message);
		});
	}
}
