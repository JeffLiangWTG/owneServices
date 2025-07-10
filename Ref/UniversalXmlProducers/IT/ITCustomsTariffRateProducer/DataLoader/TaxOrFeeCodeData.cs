using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class TaxOrFeeCodeDataWrapper
	{
		[JsonProperty("value")]
		public TaxOrFeeCodeData[] Value { get; set; }
	}

	public class TaxOrFeeCodeData
	{
		[JsonProperty("ZZF_Code")]
		public string Code { get; set; }

		[JsonProperty("ZZF_Value")]
		public float Value { get; set; }
	}
}
