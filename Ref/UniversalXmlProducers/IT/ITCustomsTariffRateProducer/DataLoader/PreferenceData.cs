using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public sealed class PreferenceDataWrapper
	{
		[JsonProperty("value")]
		public PreferenceData[] Value { get; set; }
	}

	public sealed class PreferenceData
	{
		[JsonProperty("ZZS_Preference")]
		public string Code { get; set; }

		[JsonProperty("ZZS_ZZZ_NKDataGrouping")]
		public string DataGrouping { get; set; }
	}
}
