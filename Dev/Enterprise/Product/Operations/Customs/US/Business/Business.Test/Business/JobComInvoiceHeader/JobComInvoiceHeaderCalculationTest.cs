using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		public void TestFreightInsuranceLandingChargeDutiableAllowed()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.DDP;
			invoice.JZ_InvoiceAmount = 13000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceCharge charge = invoice.Charges.AddNew();
			charge.J7_Amount = 100m;
			AssertNoMessageErrorAgainstDutiableFlag(charge, Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance);
			AssertNoMessageErrorAgainstDutiableFlag(charge, Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			AssertNoMessageErrorAgainstDutiableFlag(charge, Customs.Business.CustomsChargeTypeList.Codes.LandingCharges);
		}

		public void TestJZ_Calc_TNIWhenDutiableFreightExists()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.CAF;
			invoice.JZ_InvoiceAmount = 13000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_IsDutiable = true;
			AssertEquals("T & I", 100m, invoice.JZ_Calc_TNI);
			InvoiceCharge charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			charge2.J7_Amount = 200m;
			charge2.J7_IsDutiable = false;
			charge2.J7_AdjustedCharge = true;
			AssertEquals("T & I", 200m, invoice.JZ_Calc_TNI);
			charge2.J7_AdjustedCharge = false;
			AssertEquals("T & I", 300m, invoice.JZ_Calc_TNI);
		}

		public override void TestCalculateCIFValueWithEXW()
		{
			Assert("No EXW in US", true);
		}

		public override void TestCalculateRealInvoiceTotalWithEXW()
		{
			Assert("No EXW in US", true);
		}

		public override void TestCalculateRealInvoiceTotalWithCIP()
		{
			Assert("there is no CIP incoterm for US", true);
		}

		public override void TestCalculateCIFWithCIP()
		{
			Assert("there is no CIP incoterm for US", true);
		}

		public override void TestCalculateRealInvoiceTotalWithDDU()
		{
			Assert("there is no DDU incoterm for US", true);
		}

		public override void TestCalculateCIFWithDDU()
		{
			Assert("there is no DDU incoterm for US", true);
		}

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => JobDeclaration.New(Factory);

		void AssertNoMessageErrorAgainstDutiableFlag(InvoiceCharge charge, string chargeType)
		{
			charge.J7_ChargeType = chargeType;
			//Freight/Insurance/Landing charges are dutiable when the cost cannot be substantiated by documents issued by shipping company in US
			charge.J7_IsDutiable = true;
			AssertEquals(false, charge.J7_IsDutiableInfo.HasMessageErrors());
			charge.J7_IsDutiable = false;
			AssertEquals(false, charge.J7_IsDutiableInfo.HasMessageErrors());
		}
	}
}
