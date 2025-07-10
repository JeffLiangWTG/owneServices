using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LineChargeAggregationToInvoiceWhereInvoiceDoesntHaveTheChargeTest : TestCaseWithFactory
	{
		public void TestEnterDiscountInLineAsAmountAggregatedIntoInvoice()
		{
			invoice.JZ_InvoiceAmount = 3581.21m;
			invoiceLine.JI_LinePrice = 3607.85m;

			BaseInvoiceLineCharge lineDIS = invoiceLine.Charges.AddNew();
			lineDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			lineDIS.J7_Amount = 26.64m;
			lineDIS.J7_RX_NKCurrency = invoice.JobDeclaration.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("Invoice Line DIS aggregated into Invoice:Amount", 26.64m, invoice.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice Line DIS aggregated into Invoice:Currency", invoice.JobDeclaration.LocalCurrencyCode, invoice.GroupCharges[0].J7_RX_NKCurrency);
		}

		public void TestApportionedOverseasFreightIsIncludedInLinesForFOBInvoice()
		{
			invoice.JZ_InvoiceAmount = 10000m;
			invoiceLine.JI_LinePrice = 10000m;

			BaseInvoiceLineCharge lineOFT = invoiceLine.Charges.AddNew();
			lineOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			lineOFT.J7_Amount = 600m;
			lineOFT.J7_RX_NKCurrency = invoice.JobDeclaration.LocalCurrencyCode;
			testDec.ResumeApportionment();

			AssertEquals("OFT is not included in lines", false, lineOFT.J7_IsIncludedInITOT);
			AssertEquals("OFT apportioned from lines not included in ITOT", false, invoice.GroupCharges[0].J7_IsIncludedInITOT);

			invoice.GroupCharges[0].J7_IsIncludedInITOT = true;
			AssertEquals("FOB invoice cannot have this amount", true, invoice.GroupCharges[0].J7_IsIncludedInITOTInfo.HasMessageErrors());
		}

		public void TestOverseasFreightAndInsuranceForFOBInvoice()
		{
			invoice.JZ_InvoiceAmount = 10000m;
			invoiceLine.JI_LinePrice = 10000m;

			BaseInvoiceLineCharge lineOFT = invoiceLine.Charges.AddNew();
			lineOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			lineOFT.J7_Amount = 600m;
			lineOFT.J7_RX_NKCurrency = invoice.JobDeclaration.LocalCurrencyCode;

			BaseInvoiceLineCharge lineONS = invoiceLine.Charges.AddNew();
			lineONS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			lineONS.J7_Amount = 20m;
			lineONS.J7_RX_NKCurrency = invoice.JobDeclaration.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("Invoice Line OFT aggregated into Invoice:Amount", 600m, invoice.GroupCharges.GetCharge(lineOFT.ChargeKey).Amount);
			AssertEquals("Invoice line OFT aggregated into invoice:Curr", invoice.JobDeclaration.LocalCurrencyCode, invoice.GroupCharges.GetCharge(lineOFT.ChargeKey).Currency.Code);
			AssertEquals("Invoice Line ONS aggregated into invoice:Amount", 20m, invoice.GroupCharges.GetCharge(lineONS.ChargeKey).Amount);
			AssertEquals("Invoice line ONS aggregated into invoice:Curr", invoice.JobDeclaration.LocalCurrencyCode, invoice.GroupCharges.GetCharge(lineONS.ChargeKey).Currency.Code);

			AssertEquals("Invoice's ITOT", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("Invoice FOB Amount", 10000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("Invoice CIF amount", 10620m, invoice.JZ_Calc_CIFAmount);
		}

		public void TestOverseasFreightAndInsuranceForFOBInvoiceWithTwoLines()
		{
			invoice.JZ_InvoiceAmount = 10000m;
			invoiceLine.JI_LinePrice = 6000m;

			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 4000m;

			BaseInvoiceLineCharge lineOFT = invoiceLine.Charges.AddNew();
			lineOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			lineOFT.J7_Amount = 600m;
			lineOFT.J7_RX_NKCurrency = invoice.JobDeclaration.LocalCurrencyCode;

			BaseInvoiceLineCharge lineONS = invoiceLine.Charges.AddNew();
			lineONS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			lineONS.J7_Amount = 20m;
			lineONS.J7_RX_NKCurrency = invoice.JobDeclaration.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition:Invoice Line OFT aggregated into Invoice:Amount", 600m, invoice.GroupCharges.GetCharge(lineOFT.ChargeKey).Amount);
			AssertEquals("PreCondition:Invoice Line ONS aggregated into invoice:Amount", 20m, invoice.GroupCharges.GetCharge(lineONS.ChargeKey).Amount);

			AssertEquals("PreCondition:Invoice's ITOT", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("PreCondition:Invoice FOB Amount", 10000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("PreCondition:Invoice CIF amount", 10620m, invoice.JZ_Calc_CIFAmount);

			BaseInvoiceLineCharge line2OFT = invoiceLine2.Charges.AddNew();
			line2OFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			line2OFT.J7_Amount = 300m;
			line2OFT.J7_RX_NKCurrency = invoice.JobDeclaration.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("Invoice Line OFT aggregated into invoice:Amount", 900m, invoice.GroupCharges.GetCharge(line2OFT.ChargeKey).Amount);
			AssertEquals("Invoice's ITOT", 10000m, invoice.InvoiceLineTotal);
			AssertEquals("Invoice FOB Amount", 10000m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("Invoice CIF amount", 10920m, invoice.JZ_Calc_CIFAmount);
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceHeader invoice;
		BaseJobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000;
		}

		#endregion

	}
}
