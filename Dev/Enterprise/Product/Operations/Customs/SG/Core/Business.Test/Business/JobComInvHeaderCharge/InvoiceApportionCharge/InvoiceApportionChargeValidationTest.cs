using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class InvoiceApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestInvoiceCharge()
		{
			InvoiceApportionCharge parent = Factory.New<InvoiceApportionCharge>();
			AssertEquals(parent.Validation.InvoiceApportionCharge, parent);
		}
	}
}
