using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		public void TestInvoiceCharge()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Validation.InvoiceCharge, parent);
		}

		public new void TestValidateChargeTypeForDDPInvoice()
		{
			invoice.JZ_IncoTerm = "DDP";
			BaseInvoiceCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			AssertEquals("LCH can be on DDP invoice", false, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
			invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
			AssertEquals("ExWorks can not be on DDP invoice", true, invoiceCharge.J7_ChargeTypeInfo.HasMessageErrors());
		}

		#region Implementation
		JobDeclaration testDec;
		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		}
		#endregion
	}
}
