using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	public class InvoiceLineChargeTest : Customs.Business.Testing.BaseInvoiceLineChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update InvoiceLineChargeTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseInvoiceLineCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
