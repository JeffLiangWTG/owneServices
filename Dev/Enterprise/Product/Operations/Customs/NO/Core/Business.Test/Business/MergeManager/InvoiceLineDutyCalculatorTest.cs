using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(InvoiceLineDutyCalculator))]
sealed class InvoiceLineDutyCalculatorTest : UniversalDutyCalculatorAbstractTest<JobComInvoiceLine, InvoiceLineUniversalRateCalcData>
{
	public void TestCalculateWithCompleteLines()
	{
		var data = RateCalculatorDataHelper.SetupRefDataForDutyCalculation(Factory);
		AssertEquals("[Pre-Condition]: Excise Tariff One and Rate MB200", data.ExciseTariffOne.PK, data.MB200Rate.ZZ2_ZZ1_Tariff);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLine.JI_Tariff = data.ExciseTariffOne.ZZ1_TariffCode;
		invoiceLine.JI_SupplementaryCode1 = "MB200";
		invoiceLine.JI_CustomsQuantity = 10;
		invoiceLine.JI_CustomsUnitQty = "KGM";
		invoiceLine.JI_CustomsSecondQuantity = 0.02;
		invoiceLine.JI_CustomsSecondUnitQty = "LTR";
		invoiceLine.JI_CustomsThirdQuantity = 13;
		invoiceLine.JI_CustomsThirdUnitQty = "NMB";
		CalculateAndAssert(invoiceLine, data.MB200Rate.PK, expectedFinalResultAmount: 87.23m, new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(87.23, 6.71, 13, "NMB")
		});

		var invoiceLineTwo = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLineTwo.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLineTwo.JI_Tariff = data.ExciseTariffTwo.ZZ1_TariffCode;
		invoiceLineTwo.JI_SupplementaryCode1 = "MA207";
		invoiceLineTwo.JI_CustomsThirdQuantity = 10;
		invoiceLineTwo.JI_CustomsThirdUnitQty = "LTR";
		invoiceLineTwo.JI_CustomsFourthQuantity = 1;
		invoiceLineTwo.JI_CustomsFourthUnitQty = "ASV";
		CalculateAndAssert(invoiceLineTwo, data.MA207Rate.PK, expectedFinalResultAmount: 51.40m, new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(51.40, 5.14, 10, "LTR")
		});
	}

	public void TestUniversalRateCalcData()
	{
		var calculator = new InvoiceLineDutyCalculatorForTest(Factory.New<JobComInvoiceLine>(), Factory.New<RateView>());
		AssertType<InvoiceLineUniversalRateCalcData>(calculator.CreateUniversalRateCalcData_Exposed());
	}

	public void TestRateCalculationVisitor()
	{
		var calculator = new InvoiceLineDutyCalculatorForTest(Factory.New<JobComInvoiceLine>(), Factory.New<RateView>());
		AssertType<RateCalculationVisitorWithCombinedMultiplicationSubResults.Creator>(calculator.RateCalculationVisitorCreator_Exposed);
	}

	void CalculateAndAssert(JobComInvoiceLine invLine, ZGuid refCusRatePk, decimal expectedFinalResultAmount, IDutyCalculationIntermediateResult[] expectedIntermediateResults, ErrorInformation[] expectedErrors = null)
	{
		var dutyRateForCalculation = Factory.Load<RateView>(refCusRatePk);
		AssertEquals("Rate for calculation and specified Tariff on line should be same", dutyRateForCalculation.CusTariff.PK, invLine.UniversalTariff.PK);
		var assertionMessage = $"Calculation Result\r\nFormula = {dutyRateForCalculation.ZZ2_RateFormula}\r\n";
		var dutyCalculator = new InvoiceLineDutyCalculator(invLine);
		var calculationResult = dutyCalculator.CleanFormulaAndCalculate(dutyRateForCalculation);
		RateCalculationVisitorTestBase.AssertCalculation(assertionMessage, calculationResult, expectedFinalResultAmount, expectedIntermediateResults, expectedErrors);
	}

	protected override UniversalDutyCalculator<JobComInvoiceLine, InvoiceLineUniversalRateCalcData> CreateDutyCalculator()
		=> new InvoiceLineDutyCalculator(Factory.New<JobComInvoiceLine>());

	class InvoiceLineDutyCalculatorForTest(JobComInvoiceLine invLine, RateView rateView) : InvoiceLineDutyCalculator(invLine)
	{
		public IUniversalRateCalcData CreateUniversalRateCalcData_Exposed() => CreateUniversalRateCalcData(invLineRateView);

		public IRateCalculationVisitorCreator RateCalculationVisitorCreator_Exposed => GetRateCalculationVisitorCreator(rateView);

		RateView invLineRateView => rateView;
	}
}
