using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceApportionChargeValidationTest : TestCaseWithFactory
	{
		public void TestIsCIFComponentUsed()
		{
			Assert(new InvoiceApportionChargeValidationForTest(Factory.New<InvoiceApportionCharge>()).IsCIFComponentUsedExposed);
		}

		sealed class InvoiceApportionChargeValidationForTest : InvoiceApportionChargeValidation
		{
			public InvoiceApportionChargeValidationForTest(InvoiceApportionCharge invoiceLineCharge) : base(invoiceLineCharge)
			{
			}

			internal bool IsCIFComponentUsedExposed => IsCIFComponentUsed;
		}
	}
}
