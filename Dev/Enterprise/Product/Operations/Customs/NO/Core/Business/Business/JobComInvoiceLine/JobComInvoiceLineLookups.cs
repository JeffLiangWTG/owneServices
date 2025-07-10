using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.NO.Business;

public class JobComInvoiceLineLookups(JobComInvoiceLine parent) : Customs.Business.JobComInvoiceLineLookups(parent)
{
	public new readonly JobComInvoiceLine Parent = Argument.NotNull(parent, nameof(parent));

	public override CodeDescriptionPairList ValuationCodeList => Factory.GetCachedValue<ValuationMethodList>();

	public override ICodeDescriptionPairList Procedures
	{
		get
		{
			var dataGroupingCode = GetDefaultDataGroupingCode();
			var style = Parent.EntryInstruction?.CEI_Style ?? ZString.Empty;
			var messageType = Parent.Declaration?.JE_MessageType ?? ZString.Empty;
			var assessmentDate = Parent.EffectiveAssessmentDate;

			var result = Factory.GetCachedValue($"NO.JobComInvoiceLineLookups.Procedures.{messageType}.{assessmentDate}.{style}", () =>
			{
				var list = new CodeDescriptionPairList();
				new RefCusProcedureCollection(Factory, dataGroupingCode, assessmentDate, style, messageType)
					.ForEach(x => list.AddPairIfNotExist(string.Concat(x.ZZ6_ProcedureCode, x.ZZ6_PreviousProcedureCode), x.ZZ6_Description));
				return list;
			});
			return result;
		}
	}

	public CodeDescriptionPairList NOStateOrRegionOfOrigin
	{
		get
		{
			var countryOfOrigin = Parent.JI_CountryOfOrigin;
			var isCountryOfOriginNO = countryOfOrigin == Constants.CountryCodes.Norway;
			return Factory.GetCachedValue($"NO.JobComInvoiceLineLookups.NOStateOrRegionOfOrigin.IsCountryOriginNO:{isCountryOfOriginNO}", () =>
			{
				var list = new CodeDescriptionPairList();
				if (isCountryOfOriginNO)
				{
					list.AddRange(Factory.GetStateList(countryOfOrigin, false));
					list.AddRangeOverwriteIfExists(new NOStateOrRegionOfOriginList());
					list.RemoveCode(NOStateOrRegionOfOriginList.Codes._91);
					list.Sort();
				}
				else
				{
					list.AddPair(NOStateOrRegionOfOriginList.Codes._91, NOStateOrRegionOfOriginList.Descriptions._91);
					list.DefaultCode = NOStateOrRegionOfOriginList.Codes._91;
				}
				return list;
			});
		}
	}

	public CodeDescriptionPairList CustomsUnitQtyList
	{
		get
		{
			var tariff = Parent.JI_Tariff;
			return Factory.GetCachedValue($"NO.JobComInvoiceLineLookups.CustomsUnitQtyList.{tariff}", () =>
			{
				return GetUOMsFromTariff(UOMTypeList.Codes.CU1);
			});
		}
	}

	public CodeDescriptionPairList CustomsSecondUnitQtyList => Factory.GetCachedValue(CustomsSecondUnitQtyListCacheKey, () =>
	{
		var list = GetUOMsFromTariff(UOMTypeList.Codes.CU2);
		if (list.Count != 0)
		{
			return list;
		}

		return CustomsUQList;
	});

	internal string CustomsSecondUnitQtyListCacheKey => $"NO.JobComInvoiceLineLookups.CustomsSecondUnitQtyList.{Parent.JI_Tariff}";

	CodeDescriptionPairList GetUOMsFromTariff(string uomType)
	{
		var list = new CodeDescriptionPairList();
		var unitsOfMeasure = Parent.UniversalTariff
			?.UnitsOfMeasure
			?.FirstOrDefault(x => x.ZZ8_Type == uomType)?.ZZ8_UOM ?? ZString.Empty;

		if (!unitsOfMeasure.IsEmpty)
		{
			list.AddPair(unitsOfMeasure, unitsOfMeasure);
			list.DefaultCode = unitsOfMeasure;
		}

		return list;
	}

	public override CodeDescriptionPairList CustomsUQList => Factory.GetCachedValue(CustomsUQListCacheKey, GetUOMsFromFilteredRates);

	internal string CustomsUQListCacheKey => $"NO.JobComInvoiceLineLookups.CustomsUQList.{Parent.JI_Tariff}";

	CodeDescriptionPairList GetUOMsFromFilteredRates()
	{
		var list = new CodeDescriptionPairList();
		var tariffUnitsOfMeasure =
#if NETFRAMEWORK
			IEnumerableExtensions.DistinctBy(
#else
			Enumerable.DistinctBy(
#endif
				Parent.UniversalTariff
					?.FilteredRates
					?.Where(x => x.CusRateType.ZZR_RateType == parent.ExciseRateType)
					.SelectMany(x => x.UnitsOfMeasure)
					.Where(x => x.ZXG_UOM != NOCustomsFormulaUnitCodeList.Codes.VFD) ?? [],
				x => x.ZXG_UOM) ?? [];

		foreach (var unitOfMeasure in tariffUnitsOfMeasure)
		{
			list.AddPair(unitOfMeasure.ZXG_UOM, unitOfMeasure.ZXG_UOM);
		}

		return list;
	}

	public static CodeDescriptionPair FormulaUnitToRateType(string formulaUnit)
	{
		var (code, description) = formulaUnit switch
		{
			NOCustomsFormulaUnitCodeList.Codes.GRM => (RateTypeCodeList.Codes.Gram, NOCustomsFormulaUnitCodeList.Descriptions.GRM),
			NOCustomsFormulaUnitCodeList.Codes.KGM => (RateTypeCodeList.Codes.Kilogram, NOCustomsFormulaUnitCodeList.Descriptions.KGM),
			NOCustomsFormulaUnitCodeList.Codes.LTR => (RateTypeCodeList.Codes.Liter, NOCustomsFormulaUnitCodeList.Descriptions.LTR),
			NOCustomsFormulaUnitCodeList.Codes.MTQ => (RateTypeCodeList.Codes.CubicMeter, NOCustomsFormulaUnitCodeList.Descriptions.MTQ),
			NOCustomsFormulaUnitCodeList.Codes.NMB => (RateTypeCodeList.Codes.Piece, NOCustomsFormulaUnitCodeList.Descriptions.NMB),
			NOCustomsFormulaUnitCodeList.Codes.VFD => (RateTypeCodeList.Codes.PercentSign, NOCustomsFormulaUnitCodeList.Descriptions.VFD),
			_ => (formulaUnit, formulaUnit),
		};
		return new CodeDescriptionPair(code, description);
	}

	public override CodeDescriptionPairList TaxOrFeeCodeList
	{
		get
		{
			var taxList = new CodeDescriptionPairList();

			var privateList = RefCusTaxOrFee.Loader.GetList(Factory, GetDefaultDataGroupingCode(), InvoiceLine.EffectiveAssessmentDate, TaxOrFeeType).Cast<RefCusTaxOrFee>()
				.Where(t => t.ZZF_Code == UniversalReferenceConstants.RefCusTaxOrFee.MVF || t.ZZF_Code == UniversalReferenceConstants.RefCusTaxOrFee.MVK);

			taxList.AddRange(base.TaxOrFeeCodeList);
			taxList.AddPairsIfNotExist(privateList);

			return taxList;
		}
	}

	public override CodeDescriptionPairList AdditionalCodesList =>
		SupplementaryCodeHelper.GetCodeListBasedOnPackageType(GetAdditionalCodesListCore(), Parent);

	public ReadOnlyCodeDescriptionPairList CustomsOverrideTypeList
	{
		get
		{
			var tariff = InvoiceLine.JI_Tariff;
			var preference = InvoiceLine.JI_PrimaryPreference;
			return Factory.GetCachedValue($"NO.JobComInvoiceLineLookups.CustomsOverrideTypeList.{tariff}.{preference}", GetCustomsOverrideTypeListCore);
		}
	}

	CodeDescriptionPairList GetCustomsOverrideTypeListCore()
	{
		var list = new CodeDescriptionPairList();
		var rateLevelUnitOfMeasures = InvoiceLine
			?.UniversalTariff
			?.GetApplicableRates(InvoiceLine.DutyRateSelectionCriteria)
			?.SelectMany(x => x.GetUnitsOfMeasure())
			.Distinct() ?? [];

		foreach (var unitOfMeasure in rateLevelUnitOfMeasures)
		{
			var formulaUnit = JobComInvoiceLineLookups.FormulaUnitToRateType(unitOfMeasure);
			list.AddPair(formulaUnit.Code, formulaUnit.Description);
		}

		if (list.Count > 0)
		{
			list.DefaultCode = list.ContainsCode(RateTypeCodeList.Codes.Kilogram) ? RateTypeCodeList.Codes.Kilogram : list[0].Code;
		}

		return list;
	}

	public ICodeDescriptionPairList ReducedCustomsFlagList => Factory.GetCachedValue<ReducedCustomsFlagList>();

	public CodeDescriptionPairList PackageTypeList => Factory.GetCachedValue<NOPackageTypes>();

	CodeDescriptionPairList GetAdditionalCodesListCore() =>
		UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(
			InvoiceLine.UniversalTariff,
			CachedListOfAdditionalCodeDescriptions,
			[Parent.ExciseRateSelectionCriteria],
			InvoiceLine.ConditionSelectionCriterias,
			ConditionTypesToExcludeFromAdditionalCodesList,
			InvoiceLine.VATSelectionCriteria,
			InvoiceLine.TariffAdditionalCodeSelectionCriteria);

	protected override BusinessObjectFactory Factory => Parent.Factory ?? new BusinessObjectFactory();
}
