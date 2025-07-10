using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.NO.Business.Testing;

public class RefCusTariffTestHelper
{
	internal const string GroupingNorway = Core.Constants.CountryCodes.Norway;
	internal const string GeneralTradeGroupCode = "TALL";

	public static class TariffCodes
	{
		public const string TariffWithOneExciseCode = "11111111";
		public const string TariffWithTwoExciseCode = "22222222";
		public const string TariffWithMixedExciseCode = "33333333";
		public const string TariffWithOneAdditionalUom = "44444444";
		public const string TariffWithTwoAdditionalUomWhereSecondIsNmb = "55555555";
		public const string TariffWithTwoAdditionalUomWhereSecondIsStk = "19059034";
		public const string TariffWithMultipleRates = "21069060";
	}

	public RefCusTariffTestHelper(BusinessObjectFactory factory)
	{
		this.factory = factory;
		Helper = new UniversalReferenceTestDataHelper(factory);
	}
	public readonly UniversalReferenceTestDataHelper Helper;
	readonly BusinessObjectFactory factory;

	public RefCusCodeList GetOrCreateCodeListCustomsUQ(string code, string description) => GetOrCreateCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, code, description);

	public RefCusCodeList GetOrCreateCodeList(string codeType, string code, string description)
	{
		return Helper.CreateNewOrGetExistingCusCodeList(GroupingNorway, codeType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
	}

	internal TariffView CreateImportTariff(string tariffCode) => CreateTariff(tariffCode, Constants.TariffTypes.HarmonizedSystem);

	internal TariffView CreateTariff(string tariffCode, string typeCode)
	{
		var tariffType = Helper.CreateNewOrGetExistingTariffType(GroupingNorway, typeCode);
		factory.Save();

		return Helper.LoadOrCreateNewTariff(GroupingNorway, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}

	public RateView AddGeneralRate(TariffView tariff, string preference, string rateFormula)
		=> AddRateCore(tariff, preference, rateFormula, isGeneralTariffRate: true);

	public RateView AddRate(TariffView tariff, string preference, string rateFormula, params string[] countries)
		=> AddRateCore(tariff, preference, rateFormula, countries: countries);

	RateView AddRateCore(TariffView tariff, string preference, string rateFormula, bool isGeneralTariffRate = false, params string[] countries)
	{
		var rateCode = GetOrCreateRateCode(Constants.RateTypes.Duty, Constants.RateTypes.Duty);

		var preferencePR = Helper.CreatePreferenceForCountry(preference, "Preference " + preference, GroupingNorway);
		var tradeGroup = GetOrCreateTradeGroup(isGeneralTariffRate, countries);

		var rate = Helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: rateFormula, preferencePk: preferencePR.PK, dataGrouping: GroupingNorway);
		Helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		foreach (var uom in GetUnitsOfMeasure(rateFormula))
		{
			Helper.CreateRateUOM(rate.PK, uom);
		}

		factory.Save();
		return rate;
	}

	IReadOnlyCollection<string> GetUnitsOfMeasure(string rateFormula)
	{
		return rateFormula
			.Replace("VFD", "[VFD]")
			.Split('[')
			.Skip(1)
			.Select
			(
				s => s.Split(']').FirstOrDefault()
			)
			.ToList();
	}

	public VATApplicabilityView AddVatApplicability(TariffView tariff, ZString taxOrFeeCode)
		=> Helper.CreateNewOrGetExistingVATApplicability(tariff, GroupingNorway, taxOrFeeCode);

	public CusRefRateCodeView GetOrCreateRateCode(string zzrRateType, string zy1RateCode, string zy1Description = null)
	{
		var rateType = Helper.CreateNewOrGetExistingRateType(GroupingNorway, zzrRateType);
		return Helper.LoadOrCreateNewCusRateCode(factory, zy1RateCode, rateType.PK, description: zy1Description);
	}

	CusRefTradeGroupView GetOrCreateTradeGroup(bool isGeneralGroup, params string[] countries)
	{
		var tradeGroupCode = isGeneralGroup ? GeneralTradeGroupCode : $"TradeGroup#{++tradeGroupCounter}";
		var tradeGroup = Helper.LoadOrCreateTradeGroup(GroupingNorway, tradeGroupCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		if (!isGeneralGroup)
		{
			foreach (var country in countries.Where(c => !tradeGroup.TradeGroupCountries.Any(x => x.ZZB_RN_NKTradeGroupCountryCode == c)))
			{
				Helper.AddCountry(tradeGroup, country);
			}
		}
		factory.Save();
		return tradeGroup;
	}
	int tradeGroupCounter;

	public void SetupGenericTariffData()
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(GroupingNorway, Constants.TariffTypes.HarmonizedSystem);

		var tariff1 = helper.LoadOrCreateNewTariff(GroupingNorway, tariffType.PK, TariffCodes.TariffWithOneExciseCode, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff with one excise-code");
		helper.CreateTariffUOM(tariff1, UnitOfMeasureTypes.StatisticalUOMType, "KGM");
		helper.CreateTariffUOM(tariff1, UnitOfMeasureTypes.AdditionalUOMType, "LTR");
		AddExciseCodeForTariff(tariff1, "ZZ200", "0.0025 * VFD", "TestDuties # ZZ200 code");
		factory.Save();

		var tariff2 = helper.LoadOrCreateNewTariff(GroupingNorway, tariffType.PK, TariffCodes.TariffWithTwoExciseCode, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff with two excise-codes");
		helper.CreateTariffUOM(tariff2, UnitOfMeasureTypes.StatisticalUOMType, "KGM");
		AddExciseCodeForTariff(tariff2, "ZX200", "0.0025 * VFD", "TestDuties # ZX200 code", "VFD");
		AddExciseCodeForTariff(tariff2, "ZX201", "0.0050 * VFD", "TestDuties # ZZ201 code", "VFD", rateFormulaDeriveFrom: UniversalReferenceConstants.RateGivenInFractionsOfKroner);
		AddExciseCodeForTariff(tariff2, "ZX202", "5 * [NMB]", "TestDuties # ZX202 code", "NMB");
		factory.Save();

		var tariff3 = helper.LoadOrCreateNewTariff(GroupingNorway, tariffType.PK, TariffCodes.TariffWithMixedExciseCode, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff with mixed excise-codes");
		helper.CreateTariffUOM(tariff3, UnitOfMeasureTypes.StatisticalUOMType, "KGM");
		AddExciseCodeForTariff(tariff3, "BB200", "0.0025 * VFD", "TestDuties # BB200 code", "VFD");
		AddExciseCodeForTariff(tariff3, "AA201", "4 * [KGM]", "TestDuties # AA201 code", "KGM");
		AddExciseCodeForTariff(tariff3, "MG100", "4 * [KGM]", "TestDuties # MG100 code", "KGM");
		factory.Save();

		var tariff4 = helper.LoadOrCreateNewTariff(GroupingNorway, tariffType.PK, TariffCodes.TariffWithOneAdditionalUom, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff without excise-codes");
		helper.CreateTariffUOM(tariff4, UnitOfMeasureTypes.StatisticalUOMType, "KGM");
		factory.Save();

		var tariff5 = helper.LoadOrCreateNewTariff(GroupingNorway, tariffType.PK, TariffCodes.TariffWithTwoAdditionalUomWhereSecondIsNmb, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff with no excise-codes");
		helper.CreateTariffUOM(tariff5, UnitOfMeasureTypes.StatisticalUOMType, "KGM");
		helper.CreateTariffUOM(tariff5, UnitOfMeasureTypes.AdditionalUOMType, "NMB");
		factory.Save();

		var tariff6 = helper.LoadOrCreateNewTariff(GroupingNorway, tariffType.PK, TariffCodes.TariffWithTwoAdditionalUomWhereSecondIsStk, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff with CU2 UOM");
		helper.CreateTariffUOM(tariff6, UnitOfMeasureTypes.StatisticalUOMType, "KGM");
		helper.CreateTariffUOM(tariff6, UnitOfMeasureTypes.AdditionalUOMType, "STK");
		factory.Save();
	}

	public void SetupTariffWithMultipleTariffRates()
	{
		var helper = new UniversalReferenceTestDataHelper(factory);

		var dateStart = new ZDate(2010, 12, 10);
		var dateEnd = new ZDate(2079, 06, 06);

		var tradeGroupStandard = GetOrCreateTradeGroup(true, Enterprise.Core.Constants.CountryCodes.Latvia);

		var tariffType = helper.CreateNewOrGetExistingTariffType(GroupingNorway, TariffTypes.HarmonizedSystem);
		factory.Save();

		var dutyRateType = helper.CreateNewOrGetExistingRateType(GroupingNorway, RateTypes.Duty, "Duty");
		var rateCode = helper.LoadOrCreateNewCusRateCode(factory, "DTY", dutyRateType.PK);
		var preference = helper.CreatePreferenceForCountry("N", "Standard", GroupingNorway);

		var cusTariff = helper.CreateTariff(GroupingNorway, tariffType.PK, TariffCodes.TariffWithMultipleRates, dateStart, dateEnd, "Tariff with multiple rates");
		var testRate1 = helper.CreateRate(cusTariff, rateCode.PK, dateStart, dateEnd, rateFormula: "10 * [KGM]", preferencePk: preference.PK, dataGrouping: GroupingNorway);
		helper.CreateCusApplicability(testRate1, tradeGroupStandard, dateStart, dateEnd);
		helper.CreateRateUOM(testRate1.PK, "KGM");
		var testRate2 = helper.CreateRate(cusTariff, rateCode.PK, dateStart, dateEnd, rateFormula: "0.2 * VFD", preferencePk: preference.PK, dataGrouping: GroupingNorway);
		helper.CreateCusApplicability(testRate2, tradeGroupStandard, dateStart, dateEnd);
		helper.CreateRateUOM(testRate2.PK, "%");
		factory.Save();
	}

	public void AddExciseCodeForTariff(TariffView tariff, string exciseCode, string formula, string description, string uom = default, string rateFormulaDeriveFrom = "")
	{
		var rateType = Helper.CreateNewOrGetExistingRateType(GroupingNorway, Universal.Constants.RateTypes.Excise);
		var rateCode = Helper.LoadOrCreateNewCusRateCode(factory, exciseCode, rateType.PK, description: description);
		var rate = Helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, formula, rateFormulaDeriveFrom: rateFormulaDeriveFrom, dataGrouping: GroupingNorway);
		if (uom != default)
		{
			Helper.CreateRateUOM(rate.PK, uom);
		}
		Helper.CreateCusApplicability(rate.PK, null, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
	}
}
