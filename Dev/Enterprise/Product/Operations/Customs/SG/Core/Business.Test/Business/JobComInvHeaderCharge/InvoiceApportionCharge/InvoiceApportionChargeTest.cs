using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	public class InvoiceApportionChargeTest : Customs.Business.Testing.BaseApportionedChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseApportionedCharge)).GetType() == GetExpectedBusinessObjectType());
		}
	}
}
