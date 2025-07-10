using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class InvoiceApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestInvoiceCharge()
		{
			InvoiceApportionCharge parent = Factory.New<InvoiceApportionCharge>();
			AssertEquals(parent.Validation.InvoiceApportionCharge, parent);
		}
	}
}
