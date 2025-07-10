using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class RateCodeDataWrapper
	{
		[JsonProperty("value")]
		public RateCodeData[] Value { get; set; }
	}

	public class RateCodeData
	{
		[JsonProperty("ZY1_Description")]
		public string Description { get; set; }

		[JsonProperty("ZY1_RateCode")]
		public string RateCode { get; set; }

		[JsonProperty(nameof(RefCusRateType))]
		public RefCusRateType RefCusRateType { get; set; }
	}

	public class RefCusRateType
	{
		[JsonProperty("ZZR_RateType")]
		public string RateType { get; set; }

		[JsonProperty("ZZR_ZZZ_NKDataGrouping")]
		public string RateTypeDataGrouping { get; set; }
	}
}
