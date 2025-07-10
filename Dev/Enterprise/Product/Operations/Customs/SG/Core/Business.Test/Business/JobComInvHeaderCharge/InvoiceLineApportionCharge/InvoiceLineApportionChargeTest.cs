using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(InvoiceLineApportionCharge))]
	public class InvoiceLineApportionChargeTest : Customs.Business.Testing.BaseInvoiceLineApportionedChargeTest
	{
		public void TestJ7_Percentage()
		{
			Assert("See tests in class ApportionManager", true);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseInvoiceLineApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
