using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.CarrierShipmentRates.Models;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business;

sealed class RateResultDtoConverter(BusinessObjectFactory factory, IRateChooserServices chooserServices)
{
	public ICollection<CarrierShipmentRateResultDto> ConvertFromRateChargeDtoCollection(IEnumerable<(AutoRateResult autoRateResult, RatingCriteria criteria, CostSell costOrSell)> rateResults)
	{
		var relevantTaxPercentages = GetAllRelevantTaxPercentages(rateResults);
		var resultDtos = new List<CarrierShipmentRateResultDto>();
		foreach (var rateResult in rateResults)
		{
			var charges = rateResult.autoRateResult.RateInfoCollection.Where(x => x.Amount > 0).Select(x => ConvertChargeToDto(x, relevantTaxPercentages, rateResult.costOrSell)).ToList();
			var criteria = new CarrierShipmentRateCriteriaDto
			{
				Origin = rateResult.criteria.OriginCode,
				Destination = rateResult.criteria.DestinationCode
			};

			var localCurrency = rateResult.autoRateResult.RateInfoCollection.FirstOrDefault()?.LocalCurrency ?? string.Empty;

			if (charges.Count > 0)
			{
				resultDtos.Add(new CarrierShipmentRateResultDto(charges, criteria, rateResult.costOrSell, localCurrency));
			}
		}

		return resultDtos;
	}

	CarrierShipmentRateChargeDto ConvertChargeToDto(AutoRateInfo autoRateInfo, Dictionary<ZGuid, ZDecimal> relevantTaxPercentages, CostSell costOrSell)
	{
		var commodityCode = autoRateInfo.Attributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.Commodity);
		var commodity = factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, commodityCode);
		var localMoney = chooserServices.ConvertToDefaultCurrency(autoRateInfo.Amount, autoRateInfo.Currency);
		ZDecimal osCostGSTAmount = 0m;
		if (costOrSell == CostSell.Cost)
		{
			relevantTaxPercentages.TryGetValue(autoRateInfo.ChargeCode.AC_AT_GSTRate, out var osCostGSTPercentage);
			osCostGSTAmount = autoRateInfo.Amount * osCostGSTPercentage;
		}

		return new CarrierShipmentRateChargeDto
		{
			ContainerType = autoRateInfo.Attributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.ContainerCode),
			CommodityCode = commodityCode,
			CommodityDescription = commodity?.RH_DescriptionMultilingual,
			SourcePK = autoRateInfo.Line.PK.ToGuid(),
			ChargeCodePK = autoRateInfo.ChargeCode.PK.ToGuid(),
			ChargeCode = autoRateInfo.ChargeCode.AC_Code,
			ChargeCodeGroup = autoRateInfo.ChargeCode.AC_ChargeGroup,
			ChargeDescription = autoRateInfo.ChargeCode.AC_DescMultilingual.ToString(),
			ChargeUnit = autoRateInfo.ChargeUnit,
			RateCalculatorCode = autoRateInfo.Line.TL_RateCalculator,
			CalculationDescription = autoRateInfo.CalculationDescription,
			LocalAmount = localMoney.IsValid ? localMoney.Amount : 0,
			RateAmount = autoRateInfo.Amount,
			RateCurrency = autoRateInfo.Currency,
			OSCostGSTAmount = osCostGSTAmount,
			GSTRatePK = autoRateInfo.ChargeCode.AC_AT_GSTRate.ToGuid(),
			PaymentBases = ConvertPaymentBasisToDto(autoRateInfo.Bases),
			Attributes = ConvertAttributesToDto(autoRateInfo.Attributes),
		};
	}

	Dictionary<ZGuid, ZDecimal> GetAllRelevantTaxPercentages(IEnumerable<(AutoRateResult autoRateResult, RatingCriteria criteria, CostSell costOrSell)> rateResults)
	{
		var requiredTaxRatePKs = rateResults.Where(rateResult => rateResult.costOrSell == CostSell.Cost)
			.SelectMany(rateResult => rateResult.autoRateResult.RateInfoCollection.Select(v => v.ChargeCode.AC_AT_GSTRate).Where(gst => gst != ZGuid.Empty))
			.Distinct()
			.ToList();
		if (!requiredTaxRatePKs.Any())
		{
			return new Dictionary<ZGuid, ZDecimal>();
		}

		var taxRates = factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.PK, requiredTaxRatePKs));
		return taxRates.ToDictionary(key => key.PK, value => value.GetRate(ZDate.Today));
	}

	ICollection<CarrierShipmentRateAttribute> ConvertAttributesToDto(RateAttributeSet attributes)
	{
		return attributes.Attributes.Select(attr => new CarrierShipmentRateAttribute
		{
			Code = attr.Code,
			Value = attr.Value,
			Amount = attr.Amount,
		}).ToList();
	}

	ICollection<CarrierShipmentRatePaymentBasis> ConvertPaymentBasisToDto(List<PaymentBasis> paymentBases)
	{
		return paymentBases.Select(paymentBasisItem => new CarrierShipmentRatePaymentBasis
		{
			ChargeableAmount = paymentBasisItem.Chargeable.Amount,
			ChargeableUnit = paymentBasisItem.Chargeable.Unit,
			ChargeableUnitType = paymentBasisItem.Chargeable.UnitType.ToString(),
			ChargeableDescription = paymentBasisItem.Chargeable.Description,
			AdapterType = paymentBasisItem.AdapterType.ToString(),
			AdapterID = paymentBasisItem.AdapterID,
			MinRate = paymentBasisItem.RateInfo.MinRate,
			MaxRate = paymentBasisItem.RateInfo.MaxRate,
			FlatRate = paymentBasisItem.RateInfo.FlatRate ?? 0,
			PerUnitRate = paymentBasisItem.RateInfo.PerUnitRate ?? 0,
			RateCurrency = paymentBasisItem.RateInfo.Currency,
			RateUnit = paymentBasisItem.RateInfo.Unit,
			RateUnitType = GetUnitTypeFromUnit(paymentBasisItem.RateInfo.Unit),
			RateType = paymentBasisItem.RateInfo.Type.ToString(),
			RateUnitMultiplier = paymentBasisItem.RateInfo.UnitMultiplier ?? 1,
		}).ToList();
	}

	string GetUnitTypeFromUnit(string unit)
	{
		if (Core.Constants.Weight.ContainsCode(unit))
		{
			return nameof(ZUnitType.Weight);
		}

		if (Core.Constants.Volume.ContainsCode(unit))
		{
			return nameof(ZUnitType.Volume);
		}

		if (Core.Constants.Length.ContainsCode(unit))
		{
			return nameof(ZUnitType.Length);
		}

		return string.Empty;
	}
}
