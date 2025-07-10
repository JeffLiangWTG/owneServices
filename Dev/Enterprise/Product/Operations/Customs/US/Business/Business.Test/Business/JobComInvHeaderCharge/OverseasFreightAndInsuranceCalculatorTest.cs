using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class OverseasFreightAndInsuranceCalculatorTest : TestCaseWithFactory
	{
		public void TestGetCharge()
		{
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.CAF;
			invoice.JZ_InvoiceAmount = 13000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_IsDutiable = true;
			AssertEquals(100m, new OverseasFreightAndInsuranceCalculator().GetCharge(invoice, new string[] { USCustomsChargeTypeList.Codes.OverseasFreight }).Amount);
			InvoiceCharge charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge2.J7_Amount = 200m;
			charge2.J7_IsDutiable = false;
			charge2.J7_AdjustedCharge = true;
			AssertEquals(200m, new OverseasFreightAndInsuranceCalculator().GetCharge(invoice, new string[] { USCustomsChargeTypeList.Codes.OverseasFreight }).Amount);
			charge2.J7_AdjustedCharge = false;
			AssertEquals(300m, new OverseasFreightAndInsuranceCalculator().GetCharge(invoice, new string[] { USCustomsChargeTypeList.Codes.OverseasFreight }).Amount);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
		}
	}
}
