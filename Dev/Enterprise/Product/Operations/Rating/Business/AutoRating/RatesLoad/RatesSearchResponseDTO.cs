using WiseRates.Api.Model;

namespace Enterprise.Rating.Business
{
	public class RatesSearchResponseDTO
	{
		public RatesSearchResponse RatesSearchResponse { get; set; }
		public string RawResponse { get; set; }
		public string TraceID { get; set; }
	}
}
