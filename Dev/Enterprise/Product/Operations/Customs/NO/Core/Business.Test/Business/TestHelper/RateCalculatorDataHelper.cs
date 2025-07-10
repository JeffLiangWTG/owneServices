using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business.Testing;

static class RateCalculatorDataHelper
{
	public static RateCalculationReferenceData SetupRefDataForDutyCalculation(BusinessObjectFactory factory)
	{
		var startDate = ZDate.Today.AddYears(-1);
		var endDate = ZDate.Today.AddYears(1);
		var refDataHelper = new UniversalReferenceTestDataHelper(factory);

		var dataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var tradeGroup = refDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", startDate, endDate);
		var harmonizedTariff = refDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
		var preference = refDataHelper.CreatePreferenceView("STD", "Standard", dataGrouping);
		factory.Save();

		var dtyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
		var exciseRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise, "Excise");
		var exportExciseRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.ExportTaxes, "Export Excise");

		var tariffOne = refDataHelper.CreateTariff(dataGrouping, harmonizedTariff.PK, "11111111", startDate, endDate, taxOrFeeCode: "DTY");
		var tariffTwo = refDataHelper.CreateTariff(dataGrouping, harmonizedTariff.PK, "11111133", startDate, endDate, taxOrFeeCode: "DTY");

		var rtOneRateCode = refDataHelper.LoadOrCreateNewCusRateCode(factory, "RT1", dtyRateType.PK);
		var rtOneRate = refDataHelper.CreateRefCusRate(tariffOne.PK, rtOneRateCode.PK, startDate, endDate, RateOneFormula, preference.PK);

		var rtTwoRateCode = refDataHelper.LoadOrCreateNewCusRateCode(factory, "RT2", dtyRateType.PK);
		var rtTwoRate = refDataHelper.CreateRefCusRate(tariffTwo.PK, rtTwoRateCode.PK, startDate, endDate, RateTwoFormula, preference.PK);

		var exciseOne = refDataHelper.CreateTariff(dataGrouping, harmonizedTariff.PK, "01231230", startDate, endDate, taxOrFeeCode: "EXC");
		var exciseTwo = refDataHelper.CreateTariff(dataGrouping, harmonizedTariff.PK, "02431230", startDate, endDate, taxOrFeeCode: "EXC");

		var mb200RateCode = refDataHelper.LoadOrCreateNewCusRateCode(factory, "MB200", exciseRateType.PK);
		var mb200Rate = refDataHelper.CreateRefCusRate(exciseOne.PK, mb200RateCode.PK, startDate, endDate, MB200RateFormula, preference.PK);

		var mb220RateCode = refDataHelper.LoadOrCreateNewCusRateCode(factory, "MB220", exciseRateType.PK);
		var mb220Rate = refDataHelper.CreateRefCusRate(exciseOne.PK, mb220RateCode.PK, startDate, endDate, MB220RateFormula, preference.PK);

		var ma207RateCode = refDataHelper.LoadOrCreateNewCusRateCode(factory, "MA207", exciseRateType.PK);
		var ma207Rate = refDataHelper.CreateRefCusRate(exciseTwo.PK, ma207RateCode.PK, startDate, endDate, MA207RateFormula, preference.PK);

		var mixedTariff = refDataHelper.CreateTariff(dataGrouping, harmonizedTariff.PK, "18063100", startDate, endDate);
		var rt200RateCode = refDataHelper.LoadOrCreateNewCusRateCode(factory, "RT200", exciseRateType.PK);
		var rt200Rate = refDataHelper.CreateRefCusRate(mixedTariff.PK, rt200RateCode.PK, startDate, endDate, RT200RateFormula, preference.PK);
		var exportExciseRateOne = refDataHelper.LoadOrCreateNewCusRateCode(factory, "EX101", exportExciseRateType.PK);
		var ex101Rate = refDataHelper.CreateRefCusRate(mixedTariff.PK, exportExciseRateOne.PK, startDate, endDate, EX101RateFormula, preference.PK);
		var mixedTariffDutyRateCode = refDataHelper.LoadOrCreateNewCusRateCode(factory, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, dtyRateType.PK);
		var mixedTariffDutyRate = refDataHelper.CreateRefCusRate(mixedTariff.PK, mixedTariffDutyRateCode.PK, startDate, endDate, MixedTariffDutyRateFormula, preference.PK);

		factory.Save();

		refDataHelper.CreateCusApplicability(mb200Rate.PK, tradeGroup, startDate, endDate, "MB200");
		refDataHelper.CreateCusApplicability(mb220Rate.PK, tradeGroup, startDate, endDate, "MB220");
		refDataHelper.CreateCusApplicability(ma207Rate.PK, tradeGroup, startDate, endDate, "MA207");
		refDataHelper.CreateCusApplicability(ex101Rate.PK, tradeGroup, startDate, endDate, "EX101");
		refDataHelper.CreateCusApplicability(rt200Rate.PK, tradeGroup, startDate, endDate, "RT200");
		refDataHelper.CreateCusApplicability(mixedTariffDutyRate.PK, tradeGroup, startDate, endDate);

		factory.Save();

		return new(tariffOne, tariffTwo, exciseOne, exciseTwo, mixedTariff, preference, mb200Rate, mb220Rate, ma207Rate, rtOneRate, rtTwoRate, ex101Rate, rt200Rate, mixedTariffDutyRate);
	}

	public readonly record struct RateCalculationReferenceData(
		TariffView DutyTariffOne,
		TariffView DutyTariffTwo,
		TariffView ExciseTariffOne,
		TariffView ExciseTariffTwo,
		TariffView MixedTariff,
		CusRefPreferenceView Preference,
		RefCusRate MB200Rate,
		RefCusRate MB220Rate,
		RefCusRate MA207Rate,
		RefCusRate RateOne,
		RefCusRate RateTwo,
		RefCusRate EX101Rate,
		RefCusRate RT200Rate,
		RefCusRate MixedTariffDutyRate)
	{
		public TariffView DutyTariffOne { get; } = DutyTariffOne;
		public TariffView DutyTariffTwo { get; } = DutyTariffTwo;
		public TariffView ExciseTariffOne { get; } = ExciseTariffOne;
		public TariffView ExciseTariffTwo { get; } = ExciseTariffTwo;
		public TariffView MixedTariff { get; } = MixedTariff;
		public CusRefPreferenceView Preference { get; } = Preference;

		/// <summary>Import Declaration, Rate Formula: <inheritdoc cref="MB200RateFormula"/>, for this RefCusRate please use <see cref="ExciseTariffOne"/></summary>
		public RefCusRate MB200Rate { get; } = MB200Rate;

		/// <summary>Import Declaration, Rate Formula: <inheritdoc cref="MB220RateFormula"/>, for this RefCusRate please use <see cref="ExciseTariffOne"/></summary>
		public RefCusRate MB220Rate { get; } = MB220Rate;

		/// <summary>Import Declaration, Rate Formula: <inheritdoc cref="MA207RateFormula"/>, for this RefCusRate please use <see cref="ExciseTariffTwo"/></summary>
		public RefCusRate MA207Rate { get; } = MA207Rate;

		/// <summary>Import Declaration, Rate Formula: <inheritdoc cref="RateOneFormula"/>, for this RefCusRate please use <see cref="DutyTariffOne"/></summary>
		public RefCusRate RateOne { get; } = RateOne;

		/// <summary>Import Declaration, Rate Formula: <inheritdoc cref="RateTwoFormula"/>, for this RefCusRate please use <see cref="DutyTariffTwo"/></summary>
		public RefCusRate RateTwo { get; } = RateTwo;

		/// <summary>Export Declaration, Rate Formula: <inheritdoc cref="EX101RateFormula"/>, for this RefCusRate please use <see cref="MixedTariff"/></summary>
		public RefCusRate EX101Rate { get; } = EX101Rate;

		/// <summary>Import Declaration, Rate Formula: <inheritdoc cref="RT200RateFormula"/>, for this RefCusRate please use <see cref="MixedTariff"/></summary>
		public RefCusRate RT200Rate { get; } = RT200Rate;

		/// <summary>Import Declaration, Rate Formula: <inheritdoc cref="MixedTariffDutyRateFormula"/>, for this RefCusRate please use <see cref="MixedTariff"/>. This is a rate of type DTY, therefore cannot be specified against excise code fields.</summary>
		public RefCusRate MixedTariffDutyRate { get; } = MixedTariffDutyRate;
	}

	/// <summary>6.71 * [NMB]</summary>
	const string MB200RateFormula = "6.71 * [NMB]";
	/// <summary>1.29 * [NMB]</summary>
	const string MB220RateFormula = "1.29 * [NMB]";
	/// <summary>5.14 * [LTR] * [ASV]</summary>
	const string MA207RateFormula = "5.14 * [LTR] * [ASV]";
	/// <summary>6.91 * [KGM]</summary>
	const string RateOneFormula = "6.91 * [KGM]";
	/// <summary>0.051 * VFD</summary>
	const string RateTwoFormula = "0.051 * VFD";
	/// <summary>0.1532 * [PCS]</summary>
	const string EX101RateFormula = "0.1532 * [PCS]";
	/// <summary>7.69 * [KGM]</summary>
	const string RT200RateFormula = "7.69 * [KGM]";
	/// <summary>0.85 * [KGM]</summary>
	const string MixedTariffDutyRateFormula = "0.85 * [KGM]";
}
