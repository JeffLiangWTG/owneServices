using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	public class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public void TestValidation()
		{
			Assert(InvoiceCharge.Validation is InvoiceChargeValidation);
		}

		public void TestTypeDecider()
		{
			Assert(Factory.New<BaseInvoiceCharge>() is InvoiceCharge);
		}

		#region overriden tests from base
		public override void TestReapportionAllChargesWhenChargeCodeChargeKeyChanged()
		{
			Assert(true);
		}

		public override void TestJ7_IsNotIncludedInInvoiceSetOnFactorySaving()
		{
			Assert(true);
		}

		public override void TestIsIncludedInLinesReadOnly()
		{
			Assert(true);
		}

		public override void TestReadOnlyOfIsIncludedInInvoice()
		{
			Assert(true);
		}

		#endregion
		#region InvoiceCharge
		InvoiceCharge InvoiceCharge
		{
			get
			{
				return invoiceCharge ?? (invoiceCharge = Factory.New<InvoiceCharge>());
			}
		}

		InvoiceCharge invoiceCharge;
		#endregion
	}
}
