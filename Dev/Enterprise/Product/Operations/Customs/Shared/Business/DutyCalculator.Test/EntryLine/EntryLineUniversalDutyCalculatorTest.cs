using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(EntryLineUniversalDutyCalculator))]
[CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Latvia)]
sealed class EntryLineUniversalDutyCalculatorTest : UniversalDutyCalculatorAbstractTest<CusEntryLine, EntryLineUniversalRateCalcData>
{
	public void TestSetCustomsValueFormula()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);

			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DEV");
			rateType1.ZZR_CustomsValueFormula = "STATVAL";
			var rateCode1 = helper.CreateCusRateCode(Factory, "111", rateType1.PK);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, "IMP");
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "10000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateView = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "{\"Precalcule\"}");
			Factory.Save();

			var declaration = CreateImportJobDeclaration();
			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 500.50;
			entryLine.CL_StatisticalValue = 1000.00;

			var formula = "0.5 * VFD";
			var dutyCalculator = new EntryLineUniversalDutyCalculatorForTesting(entryLine);
			var calculationResult = dutyCalculator.CalculateFromFormula(formula, rateView);

			AssertEquals(500m, calculationResult.IntermediateResults.FirstOrDefault().Amount);
		}
	}

	public void TestCalculateWithNoFormulaCleansing()
	{
		var rateForCalculation = Factory.New<RateView>();
		rateForCalculation.ZZ2_RateFormula = "0.1234 * [GRM]";

		var declaration = CreateImportJobDeclaration();
		var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine1.JI_CustomsQuantity = 2.5;
		invLine1.JI_CustomsUnitQty = "KGM";

		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);

		CalculateAndAssert(
			entryLine,
			rateForCalculation,
			expectedFinalResultAmount: 308.5m,
			expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
			{
				new DutyCalculationIntermediateResult(308.5m, 0.1234m, 2500m, "GRM"),
			}
		);
	}

	public void TestCalculateIncludingNonParticipatingMinMaxResults()
	{
		var rateForCalculation = Factory.New<RateView>();
		rateForCalculation.ZZ2_RateFormula = "MIN(IF(VFD < 10, VFD * 0.2, VFD * 0.15), 0.12 * [KGM])";

		var declaration = CreateImportJobDeclaration();
		var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine1.JI_CustomsQuantity = 10;
		invLine1.JI_CustomsUnitQty = "KGM";

		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);
		entryLine.CL_CustomsValue = 8;

		var expectedIntermediateResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(1.6m, 0.2m, 8m,  "%") { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(1.2m, 0.12m, 10m, "KGM") { ParticipatingExpression = true },
		};

		var dutyCalculator = new EntryLineUniversalDutyCalculator(entryLine, RateCalculationVisitorMode.IncludeNotParticipatingMinMaxResults);
		var calculationResult = dutyCalculator.CleanFormulaAndCalculate(rateForCalculation);

		RateCalculationVisitorTestBase.AssertCalculation(
			assertionMessage: $"Calculation Including Non-Participating Min & Max Results\r\nFormula = {rateForCalculation.ZZ2_RateFormula}\r\n",
			calculationResult,
			expectedFinalResultAmount: 1.2m,
			expectedIntermediateResults,
			expectedErrors: null
		);
	}

	public void TestCalculateWithFormulaErrors()
	{
		var rateForCalculation = Factory.New<RateView>();
		rateForCalculation.ZZ2_RateFormula = "10*@&%";

		// Sets up entry line with invoice line data
		var declaration = CreateImportJobDeclaration();
		var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);

		CalculateAndAssert(
			entryLine,
			rateForCalculation,
			expectedFinalResultAmount: 0m,
			expectedIntermediateResults: Array.Empty<IDutyCalculationIntermediateResult>(),
			expectedErrors: new ErrorInformation[]
			{
				new ErrorInformation(FormulaVisitErrorType.SyntaxError, "Syntax error at line 1 position 5."),
			}
		);
	}

	public void TestCalculateWithZeroFormula()
	{
		var rateForCalculation = Factory.New<RateView>();
		rateForCalculation.ZZ2_RateFormula = "0";

		var declaration = CreateImportJobDeclaration();
		var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);
		entryLine.CL_CustomsValue = 258.37m;

		CalculateAndAssert(
			entryLine,
			rateForCalculation,
			expectedFinalResultAmount: 0m,
			expectedIntermediateResults: new IDutyCalculationIntermediateResult[]
			{
				new DutyCalculationIntermediateResult(0m, 0m, 258.37m, "%"),
			}
		);
	}

	public void TestCalculateWithGivenRateFormula()
	{
		var declaration = CreateImportJobDeclaration();
		var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);
		entryLine.CL_CustomsValue = 500.50;

		var formula = "0.5 * VFD";
		var dutyCalculator = new EntryLineUniversalDutyCalculatorForTesting(entryLine);
		var calculationResult = dutyCalculator.CalculateFromFormula(formula, null);

		var expectedIntermediateResults = new IDutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(250.25m, 0.5m, 500.50m, "%"),
		};
		RateCalculationVisitorTestBase.AssertCalculation($"Calculation Result\r\nFormula = {formula}\r\n", calculationResult, 250.25m, expectedIntermediateResults);
	}

	protected override UniversalDutyCalculator<CusEntryLine, EntryLineUniversalRateCalcData> CreateDutyCalculator()
		=> new EntryLineUniversalDutyCalculator(Factory.New<CusEntryLine>(), RateCalculationVisitorMode.Default);

	BaseJobDeclaration CreateImportJobDeclaration()
	{
		var declaration = Factory.New<BaseJobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		return declaration;
	}

	void CalculateAndAssert(CusEntryLine entryLine, RateView dutyRateForCalculation, decimal expectedFinalResultAmount, IDutyCalculationIntermediateResult[] expectedIntermediateResults, ErrorInformation[] expectedErrors = null)
	{
		var assertionMessage = $"Calculation Result\r\nFormula = {dutyRateForCalculation.ZZ2_RateFormula}\r\n";
		var dutyCalculator = new EntryLineUniversalDutyCalculatorForTesting(entryLine);
		var calculationResult = dutyCalculator.CleanFormulaAndCalculate(dutyRateForCalculation);
		RateCalculationVisitorTestBase.AssertCalculation(assertionMessage, calculationResult, expectedFinalResultAmount, expectedIntermediateResults, expectedErrors);
	}

	class EntryLineUniversalDutyCalculatorForTesting : EntryLineUniversalDutyCalculator
	{
		public EntryLineUniversalDutyCalculatorForTesting(CusEntryLine entryLine) : base(entryLine, RateCalculationVisitorMode.Default) { }

		public IDutyCalculationResult CalculateFromFormula(ZString rateFormula, RateView rateViewForCalculation) => Calculate(rateFormula, rateViewForCalculation);
	}
}
