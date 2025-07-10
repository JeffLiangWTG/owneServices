using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc, Integration.Customs.NL.IDepartureCargoDesc
{
	public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsHeader Header => (NctsHeader)HeaderCore;

	public override ZDecimal BY_NetWeight
	{
		get => base.BY_NetWeight;
		set
		{
			var oldValue = base.BY_NetWeight;
			base.BY_NetWeight = value;
			if (!IsCopying && oldValue != value && value == ZDecimal.Zero && Header.CALCalculationMethod == CalculationMethodList.Codes.WGT)
			{
				UpdateAllFeesFromTariffRates();
			}
		}
	}

	public override ZString BY_NetWeightUnit
	{
		get => base.BY_NetWeightUnit;
		set
		{
			var oldValue = base.BY_NetWeightUnit;
			base.BY_NetWeightUnit = value;
			if (!IsCopying && oldValue != value && Header.CALCalculationMethod == CalculationMethodList.Codes.WGT)
			{
				UpdateAllFeesFromTariffRates();
			}
		}
	}

	public override ZDecimal BY_GrossWeight
	{
		get => base.BY_GrossWeight;
		set
		{
			var oldValue = base.BY_GrossWeight;
			base.BY_GrossWeight = value;
			if (!IsCopying && oldValue != value && Header.CALCalculationMethod == CalculationMethodList.Codes.WGT)
			{
				UpdateAllFeesFromTariffRates();
			}
		}
	}

	public override ZString BY_GrossWeightUnit
	{
		get => base.BY_GrossWeightUnit;
		set
		{
			var oldValue = base.BY_GrossWeightUnit;
			base.BY_GrossWeightUnit = value;
			if (!IsCopying && oldValue != value && Header.CALCalculationMethod == CalculationMethodList.Codes.WGT)
			{
				UpdateAllFeesFromTariffRates();
			}
		}
	}

	public void NotifyChangeOfCAL(ZString newValue, ZString oldValue)
	{
		if (newValue != oldValue)
		{
			if (newValue != CalculationMethodList.Codes.WGT)
			{
				ClearFees(CommonCargoDescChargeTypeList.Codes.Duty, CalculationMethodList.Codes.WGT);
			}
			else
			{
				ClearNonMatchingFees(CommonCargoDescChargeTypeList.Codes.Duty, CalculationMethodList.Codes.WGT);
			}

			if (newValue == CalculationMethodList.Codes.WGT || oldValue == CalculationMethodList.Codes.WGT)
			{
				UpdateAllFeesFromTariffRates();
			}
		}
	}

	public void NotifyChangeOfExportTransportModeFallbackOnInlandTransportMode(ZString newValue, ZString oldValue)
	{
		if (Header.CALCalculationMethod == CalculationMethodList.Codes.WGT && newValue != oldValue)
		{
			var changedFromOrToAir = newValue == TransportModeCodeList.Codes.Air || oldValue == TransportModeCodeList.Codes.Air;
			var changedFromOrToSea = newValue == TransportModeCodeList.Codes.Sea || oldValue == TransportModeCodeList.Codes.Sea;
			if (changedFromOrToAir || changedFromOrToSea)
			{
				UpdateAllFeesFromTariffRates();
			}
		}
	}

	public void NotifyChangeOfContainerMode(ZString newValue, ZString oldValue)
	{
		if (Header.CALCalculationMethod == CalculationMethodList.Codes.WGT)
		{
			var changedFromOrToCNT = newValue == Core.Constants.ContainerModes.Containerised || oldValue == Core.Constants.ContainerModes.Containerised;
			if (changedFromOrToCNT)
			{
				UpdateAllFeesFromTariffRates();
			}
		}
	}

	public override void UpdateAllFeesFromTariffRates()
	{
		ClearNonMatchingFees(ZString.Empty, ZString.Empty);

		var rateTaxOrFeeCode = GetRateTaxOrFeeCode(BY_FormattedHarmonisedTariff);
		CalculateFees(rateTaxOrFeeCode);
	}

	ZString GetRateTaxOrFeeCode(ZString tariffCode)
	{
		var rateTaxOrFeeCode = ZString.Empty;
		if (!tariffCode.IsEmpty)
		{
			var calculationMethod = Header.CALCalculationMethod;
			var transportMode = Header.MovementHeader.ExportTransportModeFallbackOnInlandTransportMode;
			if (calculationMethod == CalculationMethodList.Codes.WGT)
			{
				rateTaxOrFeeCode = (string)transportMode switch
				{
					ModeOfTransportList.Codes._4_AirTransport => GetRateTaxOrFeeCode_AIR(tariffCode),
					ModeOfTransportList.Codes._1_SeaTransport => GetRateTaxOrFeeCode_SEA(tariffCode),
					_ => NLNctsConstants.TaxOrFeeCodes.C000
				};
			}
			else
			{
				base.UpdateAllFeesFromTariffRates();
			}
		}
		return rateTaxOrFeeCode;
	}

	ZString GetRateTaxOrFeeCode_AIR(ZString tariffCode)
	{
		var rateTaxOrFeeCode = NLNctsConstants.TaxOrFeeCodes.C400;
		if (tariffCodesAnimalAndPlants.Any(x => tariffCode.StartsWith(x)))
		{
			rateTaxOrFeeCode = NLNctsConstants.TaxOrFeeCodes.C414;
		}
		else if (tariffCodesMachines.Any(x => tariffCode.StartsWith(x)))
		{
			rateTaxOrFeeCode = NLNctsConstants.TaxOrFeeCodes.C480;
		}
		return rateTaxOrFeeCode;
	}

	ZString GetRateTaxOrFeeCode_SEA(ZString tariffCode)
	{
		var rateTaxOrFeeCode = ZString.Empty;
		if (Header.DepartureHeaderContainers.Any(x => x.IsContainerised))
		{
			rateTaxOrFeeCode = NLNctsConstants.TaxOrFeeCodes.C100;
			if (tariffCodesAnimalAndPlants.Any(x => tariffCode.StartsWith(x)))
			{
				rateTaxOrFeeCode = NLNctsConstants.TaxOrFeeCodes.C114;
			}
			else if (tariffCodesMachines.Any(x => tariffCode.StartsWith(x)))
			{
				rateTaxOrFeeCode = NLNctsConstants.TaxOrFeeCodes.C180;
			}
		}
		return rateTaxOrFeeCode;
	}

	void CalculateFees(ZString rateTaxOrFeeCode)
	{
		var baseValue = !BY_NetWeight.IsEmpty ? NetMassInKilograms : GrossMassInKilograms;
		if (!rateTaxOrFeeCode.IsEmpty)
		{
			var rate = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(DataGroupingCode, rateTaxOrFeeCode, ZDateTime.Today)?.ZZF_Value ?? ZDecimal.Zero;
			if (!rate.IsEmpty)
			{
				var newFees = new List<FeeData>() { new FeeData(baseValue, CalculationMethodList.Codes.WGT, baseValue * rate, rate) };
				UpdateFeesForType(CommonCargoDescChargeTypeList.Codes.Duty, newFees);
			}
		}
	}

	void ClearFees(ZString chargeType, ZString methodOfCalculation)
	{
		var feesToDelete = Fees.Where(f => f.BFE_ChargeType == chargeType && f.BFE_MethodOfCalculation == methodOfCalculation).ToList();
		feesToDelete.ForEach(Fees.Delete);
	}

	void ClearNonMatchingFees(ZString chargeType, ZString methodOfCalculation)
	{
		var feesToDelete = Fees.Where(f => f.BFE_ChargeType != chargeType || f.BFE_MethodOfCalculation != methodOfCalculation).ToList();
		feesToDelete.ForEach(Fees.Delete);
	}

	static readonly ImmutableHashSet<string> tariffCodesAnimalAndPlants = ImmutableHashSet.Create("01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14");
	static readonly ImmutableHashSet<string> tariffCodesMachines = ImmutableHashSet.Create("84", "85");
}
