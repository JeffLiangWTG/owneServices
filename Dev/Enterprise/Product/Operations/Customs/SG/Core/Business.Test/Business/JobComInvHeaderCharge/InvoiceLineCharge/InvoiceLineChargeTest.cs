using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	public class InvoiceLineChargeTest : Customs.Business.Testing.BaseInvoiceLineChargeTest
	{
		public void TestSetDefaultValues()
		{
			InvoiceLineCharge invoiceLineCharge = Factory.New<InvoiceLineCharge>();
			AssertEquals(true, invoiceLineCharge.J7_IsDutiable);
			AssertEquals(true, invoiceLineCharge.J7_IsGSTApplicable);
		}

		public void TestTypeDecider()
		{
			Assert(Factory.New<BaseInvoiceLineCharge>() is InvoiceLineCharge);
		}
	}
}
