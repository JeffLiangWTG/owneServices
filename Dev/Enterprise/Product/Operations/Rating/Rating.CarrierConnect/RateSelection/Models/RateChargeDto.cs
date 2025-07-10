using System;
using System.Diagnostics;
using Enterprise.Rating.Business;
using Newtonsoft.Json;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	[DebuggerDisplay("{ChargeCode}-{ChargeCodeGroup}-{LocalAmount}-{CalculationDescription}")]
	public class RateChargeDto
	{
		public Guid ChargeID { get; set; }

		public Guid SourcePK { get; set; }

		public ChargeCodeDto ChargeCode { get; set; }

		public CommodityDto Commodity { get; set; }

		public string ChargeUnit { get; set; }

		public string CalculationDescription { get; set; }

		public string CommodityCode { get; set; }

		public string ContainerType { get; set; }

		public string ContainerQuality { get; set; }

		public string Description { get; set; }

		public string HandlingOfficeName { get; set; }

		public string LocalCurrency { get; set; }

		public decimal LocalAmount { get; set; }

		public string RateCurrency { get; set; }

		public decimal RateAmount { get; set; }

		public string ChargeType { get; set; }

		public bool IsOptional { get; set; }

		public bool IsInclusiveCalculator { get; set; }

		public string[] Route { get; set; }

		[JsonIgnore]
		public AutoRateInfo AutoRateInfo { get; set; }
	}
}
