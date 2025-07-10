using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EntryLineDutyCalculator))]
sealed class EntryLineDutyCalculatorTest : UniversalDutyCalculatorAbstractTest<CusEntryLine, EntryLineUniversalRateCalcData> 
{
	public void TestCalculateWithoutExciseCodes()
	{
		var data = RateCalculatorDataHelper.SetupRefDataForDutyCalculation(Factory);
		CombineAssertions("[Pre-Condition] Tariffs and Rates Configuration", () =>
		{
			AssertEquals("Tariff One and Rate One", data.DutyTariffOne.PK, data.RateOne.ZZ2_ZZ1_Tariff);
			AssertEquals("Tariff Two and Rate Two", data.DutyTariffTwo.PK, data.RateTwo.ZZ2_ZZ1_Tariff);
		});

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoiceLineOne = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLineOne.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLineOne.JI_Tariff = data.DutyTariffOne.ZZ1_TariffCode;
		invoiceLineOne.JI_CustomsQuantity = 10;
		invoiceLineOne.JI_CustomsUnitQty = "KGM";
		invoiceLineOne.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;
		var entryLine = declaration.CustomsEntryHeaders.AddNew()
			.MergedLines
			.AddNew();
		entryLine.InvoiceLines.Add(invoiceLineOne);

		CalculateAndAssert(entryLine, data.RateOne.PK, expectedFinalResultAmount: 69.1m, new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(69.1, 6.91, 10, "KGM")
		});
	}

	public void TestCalculateWithCompleteLines()
	{
		var data = RateCalculatorDataHelper.SetupRefDataForDutyCalculation(Factory);
		CombineAssertions("[Pre-Condition] Excise Tariffs and Rates Configuration", () =>
		{
			AssertEquals("Excise Tariff One and Rate MB200", data.ExciseTariffOne.PK, data.MB200Rate.ZZ2_ZZ1_Tariff);
			AssertEquals("Excise Tariff One and Rate MB220", data.ExciseTariffOne.PK, data.MB220Rate.ZZ2_ZZ1_Tariff);
			AssertEquals("Excise Tariff Two and Rate MA207", data.ExciseTariffTwo.PK, data.MA207Rate.ZZ2_ZZ1_Tariff);
		});

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoiceLineOne = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLineOne.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLineOne.JI_Tariff = data.ExciseTariffOne.ZZ1_TariffCode;
		invoiceLineOne.JI_SupplementaryCode1 = "MB200";
		invoiceLineOne.JI_CustomsQuantity = 10;
		invoiceLineOne.JI_CustomsUnitQty = "KGM";
		invoiceLineOne.JI_CustomsSecondQuantity = 0.02;
		invoiceLineOne.JI_CustomsSecondUnitQty = "LTR";
		invoiceLineOne.JI_CustomsThirdQuantity = 13;
		invoiceLineOne.JI_CustomsThirdUnitQty = "NMB";
		invoiceLineOne.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;

		var invoiceLineTwo = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLineTwo.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLineTwo.JI_Tariff = data.ExciseTariffOne.ZZ1_TariffCode;
		invoiceLineTwo.JI_SupplementaryCode1 = "MB220";
		invoiceLineTwo.JI_CustomsQuantity = 10;
		invoiceLineTwo.JI_CustomsUnitQty = "KGM";
		invoiceLineTwo.JI_CustomsSecondQuantity = 0.04;
		invoiceLineTwo.JI_CustomsSecondUnitQty = "LTR";
		invoiceLineTwo.JI_CustomsThirdQuantity = 15;
		invoiceLineTwo.JI_CustomsThirdUnitQty = "NMB";
		invoiceLineTwo.JI_ZZF_NKTaxType = UniversalReferenceConstants.RefCusTaxOrFee.MV1;

		var entryLine = declaration.CustomsEntryHeaders.AddNew()
			.MergedLines
			.AddNew();
		entryLine.InvoiceLines.Add(invoiceLineOne);
		entryLine.InvoiceLines.Add(invoiceLineTwo);

		CalculateAndAssert(entryLine, data.MB200Rate.PK, expectedFinalResultAmount: 187.88m, new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(187.88, 6.71, 28, "NMB")
		});

		CalculateAndAssert(entryLine, data.MB220Rate.PK, expectedFinalResultAmount: 36.12m, new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(36.12, 1.29, 28, "NMB")
		});
	}

	public void TestRateCalculationVisitor()
	{
		var calculator = new EntryLineDutyCalculatorForTest(Factory.New<CusEntryLine>(), Factory.New<RateView>());
		AssertType<RateCalculationVisitorWithCombinedMultiplicationSubResults.Creator>(calculator.RateCalculationVisitorCreator_Exposed);
	}

	public void TestUniversalRateCalcData()
	{
		var calculator = new EntryLineDutyCalculatorForTest(Factory.New<CusEntryLine>(), Factory.New<RateView>());
		AssertType<EntryLineUniversalRateCalcData>(calculator.CreateUniversalRateCalcData_Exposed());
	}

	protected override UniversalDutyCalculator<CusEntryLine, EntryLineUniversalRateCalcData> CreateDutyCalculator()
		=> new EntryLineDutyCalculator(Factory.New<CusEntryLine>());

	void CalculateAndAssert(CusEntryLine entryLine, ZGuid refCusRatePk, decimal expectedFinalResultAmount, IDutyCalculationIntermediateResult[] expectedIntermediateResults, ErrorInformation[] expectedErrors = null)
	{
		var dutyRateForCalculation = Factory.Load<RateView>(refCusRatePk);
		AssertEquals("Rate for calculation and specified Tariff on line should be same", dutyRateForCalculation.CusTariff.PK, entryLine.RandomLine.UniversalTariff.PK);
		var assertionMessage = $"Calculation Result\r\nFormula = {dutyRateForCalculation.ZZ2_RateFormula}\r\n";
		var dutyCalculator = new EntryLineDutyCalculator(entryLine);
		var calculationResult = dutyCalculator.CleanFormulaAndCalculate(dutyRateForCalculation);
		RateCalculationVisitorTestBase.AssertCalculation(assertionMessage, calculationResult, expectedFinalResultAmount, expectedIntermediateResults, expectedErrors);
	}

	class EntryLineDutyCalculatorForTest : EntryLineDutyCalculator
	{
		public EntryLineDutyCalculatorForTest(CusEntryLine entity, RateView rateView) : base(entity)
		{
			this.rateView = rateView;
		}
		readonly RateView rateView;

		public IRateCalculationVisitorCreator RateCalculationVisitorCreator_Exposed => GetRateCalculationVisitorCreator(rateView);

		public IUniversalRateCalcData CreateUniversalRateCalcData_Exposed() => CreateUniversalRateCalcData(rateView);
	}
}
