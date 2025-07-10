using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(InvoiceLineUniversalDutyCalculator))]
[CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Latvia)]
sealed class InvoiceLineUniversalDutyCalculatorTests : UniversalDutyCalculatorAbstractTest<BaseJobComInvoiceLine, InvoiceLineUniversalRateCalcData>
{
	public void TestSetCustomsValueFormula()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Portugal);

			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.Portugal, "DEV");
			rateType1.ZZR_CustomsValueFormula = "CV * 10";
			var rateCode1 = helper.CreateCusRateCode(Factory, "111", rateType1.PK);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Portugal, UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Portugal, tariffType.PK, "10000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateView = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0.5 * VFD, 0.12 * [KGM]");
			var rateView1 = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "MIN(IF(VFD < 10, VFD * 0.2 + 0 * [KGM], VFD * 0.15 + 0 * [KGM]), 0.12 * [KGM] + 0 * VFD)");
			Factory.Save();

			var declaration = CreateImportJobDeclaration();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2m;

			var dutyCalculator = new InvoiceLineUniversalDutyCalculator(invoiceLine, RateCalculationVisitorMode.Default);
			var calculationResult = dutyCalculator.CleanFormulaAndCalculate(rateView);
			var expectedIntermediateResults = new DutyCalculationIntermediateResult[]
			{
				new DutyCalculationIntermediateResult(10m, 0.5m, 20m, UniversalReferenceConstants.MethodOfCalculation.Percentage) { ParticipatingExpression = true },
			};
			RateCalculationVisitorTestBase.AssertIntermediateResults(expectedIntermediateResults, calculationResult.IntermediateResults.ToArray());
			AssertCollectionContains(expectedIntermediateResults, c => c.MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage && c.AdjustedRate == 50m);

			invoiceLine.JI_LinePrice = 5m;
			dutyCalculator = new InvoiceLineUniversalDutyCalculator(invoiceLine, RateCalculationVisitorMode.IncludeNotParticipatingMinMaxResults);
			calculationResult = dutyCalculator.CleanFormulaAndCalculate(rateView1);
			expectedIntermediateResults = new[]
			{
				new DutyCalculationIntermediateResult(7.5m, 0.15m, 50m, UniversalReferenceConstants.MethodOfCalculation.Percentage) { ParticipatingExpression = false },
				new DutyCalculationIntermediateResult(0m, 0m, 0m, UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram) { ParticipatingExpression = false },
				new DutyCalculationIntermediateResult(0m, 0.12m, 0m, UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram) { ParticipatingExpression = true },
				new DutyCalculationIntermediateResult(0m, 0m, 50m, UniversalReferenceConstants.MethodOfCalculation.Percentage) { ParticipatingExpression = true },
			};

			RateCalculationVisitorTestBase.AssertIntermediateResults(expectedIntermediateResults, calculationResult.IntermediateResults.ToArray());

			AssertCollectionContains(expectedIntermediateResults, c => c.MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage && c.AdjustedRate == 15m);
			AssertCollectionContains(expectedIntermediateResults, c => c.MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage && c.AdjustedRate == 0m);
		}
	}

	public void TestCleanFormulaAndCalculate()
	{
		var rateForCalculation = Factory.New<RateView>();
		rateForCalculation.ZZ2_RateFormula = "0.2 * VFD, 0.12 * [KGM])";

		var declaration = CreateImportJobDeclaration();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CustomsQuantity = 10;
		invoiceLine.JI_CustomsUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		invoiceLine.JI_LinePrice = 2m;

		var dutyCalculator = new InvoiceLineUniversalDutyCalculator(invoiceLine, RateCalculationVisitorMode.Default);
		var calculationResult = dutyCalculator.CleanFormulaAndCalculate(rateForCalculation);
		var expectedIntermediateResults = new[]
		{
			new DutyCalculationIntermediateResult(0.4m, 0.2m, 2m, UniversalReferenceConstants.MethodOfCalculation.Percentage) { ParticipatingExpression = true },
		};
		RateCalculationVisitorTestBase.AssertIntermediateResults(expectedIntermediateResults, calculationResult.IntermediateResults.ToArray());
		AssertCollectionContains(expectedIntermediateResults, c => c.MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage && c.AdjustedRate == 20m);

		rateForCalculation.ZZ2_RateFormula = "MIN(IF(VFD < 10, VFD * 0.2, VFD * 0.15), 0.12 * [KGM])";
		dutyCalculator = new InvoiceLineUniversalDutyCalculator(invoiceLine, RateCalculationVisitorMode.IncludeNotParticipatingMinMaxResults);
		calculationResult = dutyCalculator.CleanFormulaAndCalculate(rateForCalculation);
		expectedIntermediateResults = new[]
		{
			new DutyCalculationIntermediateResult(0.4m, 0.2m, 2m, UniversalReferenceConstants.MethodOfCalculation.Percentage) { ParticipatingExpression = true },
			new DutyCalculationIntermediateResult(1.2m, 0.12m, 10m, UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram) { ParticipatingExpression = false },
		};
		RateCalculationVisitorTestBase.AssertIntermediateResults(expectedIntermediateResults, calculationResult.IntermediateResults.ToArray());
		AssertCollectionContains(expectedIntermediateResults, c => c.MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage && c.AdjustedRate == 20m);

		invoiceLine.JI_LinePrice = 11m;
		dutyCalculator = new InvoiceLineUniversalDutyCalculator(invoiceLine, RateCalculationVisitorMode.IncludeNotParticipatingMinMaxResults);
		calculationResult = dutyCalculator.CleanFormulaAndCalculate(rateForCalculation);
		expectedIntermediateResults = new[]
		{
			new DutyCalculationIntermediateResult(1.65m, 0.15m, 11m, UniversalReferenceConstants.MethodOfCalculation.Percentage) { ParticipatingExpression = false },
			new DutyCalculationIntermediateResult(1.2m, 0.12m, 10m, UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram) { ParticipatingExpression = true },
		};
		AssertCollectionContains(expectedIntermediateResults, c => c.MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage && c.AdjustedRate == 15m);
	}

	public void TestCalculateWithFormulaErrors()
	{
		var rateForCalculation = Factory.New<RateView>();
		rateForCalculation.ZZ2_RateFormula = "10*@&%";

		var declaration = CreateImportJobDeclaration();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var dutyCalculator = new InvoiceLineUniversalDutyCalculator(invoiceLine, RateCalculationVisitorMode.Default);
		var calculationResult = dutyCalculator.CleanFormulaAndCalculate(rateForCalculation);
		var expectedErrors = new ErrorInformation[] { new ErrorInformation(FormulaVisitErrorType.SyntaxError, "Syntax error at line 1 position 5.") };
		AssertEquals("Error Count", expectedErrors.Length, calculationResult.Errors.Count());
		AssertEquals("Error Information", expectedErrors[0], calculationResult.Errors.Single());
	}

	protected override UniversalDutyCalculator<BaseJobComInvoiceLine, InvoiceLineUniversalRateCalcData> CreateDutyCalculator()
	{
		return new InvoiceLineUniversalDutyCalculator(Factory.New<BaseJobComInvoiceLine>(), RateCalculationVisitorMode.Default);
	}

	BaseJobDeclaration CreateImportJobDeclaration()
	{
		var declaration = Factory.New<BaseJobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		return declaration;
	}
}
