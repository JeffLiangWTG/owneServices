namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	public class ChargeableParametersDto
	{
		public decimal? Weight { get; set; }
		public string WeightUnit { get; set; }
		public decimal? Volume { get; set; }
		public string VolumeUnit { get; set; }
		public decimal? LoadingMeters { get; set; }
		public string TransportMode { get; set; }
		public bool IsDomestic { get; set; }
	}
}
