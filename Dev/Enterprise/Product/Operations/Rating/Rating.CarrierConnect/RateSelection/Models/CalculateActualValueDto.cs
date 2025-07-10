namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	public class CalculateActualValueDto
	{
		public decimal? ChargeableAmount { get; set; }
		public string ChargeableUnit { get; set; }
		public decimal? Weight { get; set; }
		public string WeightUnit { get; set; }
		public decimal? Volume { get; set; }
		public string VolumeUnit { get; set; }
		public bool IsDomestic { get; set; }
		public string TransportMode { get; set; }
	}
}
