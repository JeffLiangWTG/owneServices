namespace Enterprise.Rating.Business;

public class CarrierShipmentRatePaymentBasis
{
	public decimal ChargeableAmount { get; set; } // from: AutoRateInfo.Bases.Chargeable.Amount , to: dbo.JobPaymentBasis.PBS_ChargeableAmount

	public string ChargeableUnit { get; set; } // from: AutoRateInfo.Bases.Chargeable.Unit , to: dbo.JobPaymentBasis.PBS_ChargeableUnit

	public string ChargeableUnitType { get; set; } // from: AutoRateInfo.Bases.Chargeable.UnitType , to: dbo.JobPaymentBasis.PBS_ChargeableUnitType

	public string ChargeableDescription { get; set; } // from: AutoRateInfo.Bases.Chargeable.Description , to: dbo.JobPaymentBasis.PBS_ChargeableDescription

	public string AdapterType { get; set; } // from: AutoRateInfo.Bases.AdapterType , to: dbo.JobPaymentBasis.PBS_AdapterType

	public string AdapterID { get; set; } // from: AutoRateInfo.Bases.AdapterID , to: dbo.JobPaymentBasis.PBS_AdapterID

	public decimal MinRate { get; set; } // from: AutoRateInfo.Bases.RateInfo.MinRate , to: dbo.JobPaymentBasis.PBS_MinRate

	public decimal MaxRate { get; set; } // from: AutoRateInfo.Bases.RateInfo.MaxRate , to: dbo.JobPaymentBasis.PBS_MaxRate

	public decimal FlatRate { get; set; } // from: AutoRateInfo.Bases.RateInfo.FlatRate , to: dbo.JobPaymentBasis.PBS_FlatRate

	public decimal PerUnitRate { get; set; } // from: AutoRateInfo.Bases.RateInfo.PerUnitRate , to: dbo.JobPaymentBasis.PBS_PerUnitRate

	public string RateCurrency { get; set; } // from: AutoRateInfo.Bases.RateInfo.Currency , to: dbo.JobPaymentBasis.PBS_RX_NKRateCurrency

	public string RateUnit { get; set; } // from: AutoRateInfo.Bases.RateInfo.Unit , to: dbo.JobPaymentBasis.PBS_RateUnit

	public string RateUnitType { get; set; } // calculated from: AutoRateInfo.Bases.RateInfo.Unit , to: dbo.JobPaymentBasis.PBS_RateUnitType

	public string RateType { get; set; } // from: AutoRateInfo.Bases.RateInfo.Type , to: dbo.JobPaymentBasis.PBS_RateReference

	public decimal RateUnitMultiplier { get; set; } // from: AutoRateInfo.Bases.RateInfo.UnitMultiplier , to: dbo.JobPaymentBasis.PBS_RateUnitMultiplier
}
