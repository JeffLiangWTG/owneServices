using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsValuationCalculatorTest : TestCaseWithFactory
	{
		public void TestGetAmountToAddToITOTForFOB()
		{
			AssertEquals("Amount to add to ITOT for FOB calculations", 120m, calculator.GetAmountToAddToITOTForDutiable(BaseJobComInvoiceHeader.GetLocalCurrencyFor(invoice)));
		}

		public void TestGetAmountToAddToITOTForCIF()
		{
			AssertEquals("Amount to add to ITOT for CIF calculations", 530m, calculator.GetAmountToAddToITOTForVatableGstable(BaseJobComInvoiceHeader.GetLocalCurrencyFor(invoice)));
		}

		public void TestGetOverseasFreight()
		{
			AssertEquals("Overseas Freight", 350m, calculator.GetOverseasFreight(BaseJobComInvoiceHeader.GetLocalCurrencyFor(invoice)));
		}

		public void TestGetOverseasInsurance()
		{
			AssertEquals("Overseas Insurance", 50m, calculator.GetOverseasInsurance(BaseJobComInvoiceHeader.GetLocalCurrencyFor(invoice)));
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceHeader invoice;
		CustomsValuationCalculator calculator;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = BaseJobDeclaration.New(Factory);
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			invoice = testDec.Invoices.AddNew();

			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

			var fIFT = invoice.Charges.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 150m;
			fIFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			var pAC = invoice.Charges.AddNew();
			pAC.J7_ChargeType = CustomsChargeTypeList.Codes.PackingCost;
			pAC.J7_Amount = 250m;
			pAC.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			pAC.J7_IsIncludedInITOT = true;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 350m;
			oFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			var oNS = invoice.GroupCharges.AddNew();
			oNS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 50m;
			oNS.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

			var aDD = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 10m, testDec.LocalCurrencyCode);
			aDD.J7_IsIncludedInITOT = true;
			aDD.J7_IsDutiable = false;

			var dIS = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 20m, testDec.LocalCurrencyCode);
			dIS.J7_IsDutiable = false;

			AssertEquals("Line Total", 9870m, invoice.InvoiceLineTotal);

			calculator = new CustomsValuationCalculator(invoice);
		}

		#endregion
	}
}
