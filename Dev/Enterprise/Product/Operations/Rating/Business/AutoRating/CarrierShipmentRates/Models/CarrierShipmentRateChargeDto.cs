using System;
using System.Collections.Generic;
using System.Diagnostics;
using Enterprise.Rating.Business.AutoRating.CarrierShipmentRates.Models;

namespace Enterprise.Rating.Business;

[DebuggerDisplay("{ChargeCode}-{ChargeCodeGroup}-{LocalAmount}-{CalculationDescription}")]
public class CarrierShipmentRateChargeDto
{
	public Guid SourcePK { get; set; }

	public Guid ChargeCodePK { get; set; }

	public string ChargeCode { get; set; }

	public string ChargeCodeGroup { get; set; }

	public string ChargeDescription { get; set; }

	public string CommodityDescription { get; set; }

	public string ChargeUnit { get; set; }

	public string CalculationDescription { get; set; }

	public string RateCalculatorCode { get; set; }

	public string CommodityCode { get; set; }

	public string ContainerType { get; set; }

	public decimal LocalAmount { get; set; }

	public string RateCurrency { get; set; }

	public decimal RateAmount { get; set; }

	public decimal OSCostGSTAmount { get; set; }

	public Guid GSTRatePK { get; set; }

	public ICollection<CarrierShipmentRatePaymentBasis> PaymentBases { get; set; }

	public ICollection<CarrierShipmentRateAttribute> Attributes { get; set; }

	public override bool Equals(object obj)
	{
		return Equals(obj as CarrierShipmentRateChargeDto);
	}

	public bool Equals(CarrierShipmentRateChargeDto other)
	{
		return other != null &&
			   SourcePK == other.SourcePK &&
			   ChargeCode == other.ChargeCode &&
			   ChargeCodeGroup == other.ChargeCodeGroup &&
			   ChargeDescription == other.ChargeDescription &&
			   CommodityDescription == other.CommodityDescription &&
			   ChargeUnit == other.ChargeUnit &&
			   CalculationDescription == other.CalculationDescription &&
			   RateCalculatorCode == other.RateCalculatorCode &&
			   CommodityCode == other.CommodityCode &&
			   ContainerType == other.ContainerType &&
			   LocalAmount == other.LocalAmount &&
			   RateCurrency == other.RateCurrency &&
			   OSCostGSTAmount == other.OSCostGSTAmount;
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hash = 17;
			hash = hash * 23 + SourcePK.GetHashCode();
			hash = hash * 23 + (ChargeCode != null ? ChargeCode.GetHashCode() : 0);
			hash = hash * 23 + OSCostGSTAmount.GetHashCode();
			return hash;
		}
	}
}
