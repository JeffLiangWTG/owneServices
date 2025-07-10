using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(EntryLineDutyCalculator))]
sealed class EntryLineDutyCalculatorTest : UniversalDutyCalculatorAbstractTest<EU.Business.Declaration.CusEntryLine, EUUniversalRateCalcData>
{
	public void TestCalculate_KGMVFD()
	{
		GlbCompany.CurrentCompany.GC_IsReciprocal = true;
		var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData();
		var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
		var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "0.2 * [KGM] + 0.3 * VFD", preferencePk: stdPreference.PK);
		PopulateExchangeRateData();
		tariffDtyRateA00.Factory.Save();
		RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate);

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
		invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
		invLine1.JI_CustomsQuantity = 100;
		invLine1.JI_CustomsUnitQty = "KGM";
		invLine1.JI_CustomsSecondQuantity = 1000;
		invLine1.JI_CustomsSecondUnitQty = "GRM";
		invLine1.JI_CustomsThirdQuantity = 50;
		invLine1.JI_CustomsThirdUnitQty = "KGM";

		var supportingDocument1 = invLine1.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Type = "XXX";
		supportingDocument1.CSI_Code = "D1";

		var invLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine2.JI_PrimaryPreference = stdPreference.ZZS_Preference;
		invLine2.JI_Tariff = dtyTariff.ZZ1_TariffCode;
		invLine2.JI_CustomsQuantity = 400;
		invLine2.JI_CustomsUnitQty = "KGM";

		var invLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine3.JI_PrimaryPreference = stdPreference.ZZS_Preference;
		invLine3.JI_Tariff = dtyTariff.ZZ1_TariffCode;
		invLine3.JI_CustomsQuantity = 10;
		invLine3.JI_CustomsUnitQty = "KGM";
		invLine3.JI_CustomsSecondQuantity = 300;
		invLine3.JI_CustomsSecondUnitQty = "KGM";

		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);
		entryLine.InvoiceLines.Add(invLine2);
		entryLine.InvoiceLines.Add(invLine3);
		entryLine.CL_CustomsValue = 20;

		var dutyRateForCalculation = entryLine.RandomLine.UniversalDutyRate;
		var dutyCalculator = new EntryLineDutyCalculatorExposed(entryLine, RateCalculationVisitorMode.Default);
		var result = dutyCalculator.Calculate_Exposed("0.2 * [KGM] + 0.3 * VFD", dutyRateForCalculation);

		CombineAssertions(() =>
		{
			Assert("Contains only one intermediate result", result.IntermediateResults.Count() == 1);
			AssertEquals("Total", 103.33333333333333333333333333M, result.ResultAmount);
			var intermediateResult = result.IntermediateResults.Single();
			AssertEquals("Base value", 4.4444444444444444444444444444M, intermediateResult.BaseValue);
			AssertEquals("Empty Method Of Calculation", ZString.Empty, intermediateResult.MethodOfCalculation);
		});
	}

	public void TestCalculate_VFD()
	{
		GlbCompany.CurrentCompany.GC_IsReciprocal = true;
		var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData();
		var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
		var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "0.5 * VFD", preferencePk: stdPreference.PK);
		PopulateExchangeRateData();
		tariffDtyRateA00.Factory.Save();
		RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate);

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
		invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
		invLine1.JI_CustomsQuantity = 100;
		invLine1.JI_CustomsUnitQty = "KGM";
		invLine1.JI_CustomsSecondQuantity = 1000;
		invLine1.JI_CustomsSecondUnitQty = "GRM";
		invLine1.JI_CustomsThirdQuantity = 50;
		invLine1.JI_CustomsThirdUnitQty = "KGM";

		var supportingDocument1 = invLine1.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Type = "XXX";
		supportingDocument1.CSI_Code = "D1";

		var invLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine2.JI_PrimaryPreference = stdPreference.ZZS_Preference;
		invLine2.JI_Tariff = dtyTariff.ZZ1_TariffCode;
		invLine2.JI_CustomsQuantity = 400;
		invLine2.JI_CustomsUnitQty = "KGM";

		var invLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine3.JI_PrimaryPreference = stdPreference.ZZS_Preference;
		invLine3.JI_Tariff = dtyTariff.ZZ1_TariffCode;
		invLine3.JI_CustomsQuantity = 10;
		invLine3.JI_CustomsUnitQty = "KGM";
		invLine3.JI_CustomsSecondQuantity = 300;
		invLine3.JI_CustomsSecondUnitQty = "KGM";

		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);
		entryLine.InvoiceLines.Add(invLine2);
		entryLine.InvoiceLines.Add(invLine3);
		entryLine.CL_CustomsValue = 20;

		var dutyRateForCalculation = entryLine.RandomLine.UniversalDutyRate;
		var dutyCalculator = new EntryLineDutyCalculatorExposed(entryLine, RateCalculationVisitorMode.Default);
		var result = dutyCalculator.Calculate_Exposed("0.5 * VFD", dutyRateForCalculation);

		CombineAssertions(() =>
		{
			Assert("Contains only one intermediate result", result.IntermediateResults.Count() == 1);
			AssertEquals("Total", 2.2222222222222222222222222222M, result.ResultAmount);
			var intermediateResult = result.IntermediateResults.Single();
			AssertEquals("Base value", 4.4444444444444444444444444444M, intermediateResult.BaseValue);
			AssertEquals("Percentage Method Of Calculation", Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage,
				intermediateResult.MethodOfCalculation);
		});
	}

	protected override UniversalDutyCalculator<EU.Business.Declaration.CusEntryLine, EUUniversalRateCalcData> CreateDutyCalculator()
	{
		return new EntryLineDutyCalculatorExposed(Factory.New<CusEntryLine>(), RateCalculationVisitorMode.Default);
	}

	protected override Type ExpectedUniversalRateCalDataType => typeof(UniversalRateCalcData);

	void PopulateExchangeRateData()
	{
		var exchangeRate = RefExchangeRate.New(Factory);
		exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
		exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
		exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		exchangeRate.RE_SellRate = 4.5m;
		exchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
		exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

		Factory.Save();
	}

	(CusRefTradeGroupView, CusRefPreferenceView, TariffView, RefCusRateType) PopulateTestReferenceData()
	{
		const string grouping = Core.Constants.CountryCodes.Poland;
		var stdTradeGroup = RefDataHelper.CreateTradeGroup(grouping, "STANDARD", startDate, endDate);
		var impTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(grouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
		var stdPreference = RefDataHelper.CreatePreferenceView("STD", "Standard", grouping);
		Factory.Save();
		var dtyTariff = RefDataHelper.CreateTariff(grouping, impTariffType.PK, "1111111", startDate, endDate, taxOrFeeCode: "DTY");
		var dtyRateType = RefDataHelper.CreateNewOrGetExistingRateType(grouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
		return (stdTradeGroup, stdPreference, dtyTariff, dtyRateType);
	}

	UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
	UniversalReferenceTestDataHelper refDataHelper;

	readonly ZDateTime startDate = ZDateTime.Today.AddYears(-1);
	readonly ZDateTime endDate = ZDateTime.Today.AddYears(1);

	class EntryLineDutyCalculatorExposed : EntryLineDutyCalculator
	{
		public EntryLineDutyCalculatorExposed(CusEntryLine entryLine, RateCalculationVisitorMode mode) : base(entryLine, mode)
		{
		}

		public IDutyCalculationResult Calculate_Exposed(ZString rateFormula, RateView rateForCalculation) => base.Calculate(rateFormula, rateForCalculation);
	}
}
