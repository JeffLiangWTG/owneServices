using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class TradeGroupDataWrapper
	{
		[JsonProperty("value")]
		public TradeGroupData[] Value { get; set; }
	}

	public class TradeGroupData
	{
		[JsonProperty("ZZA_TradeGroup")]
		public string Code { get; set; }

		[JsonProperty("ZZA_Description")]
		public string Description { get; set; }
	}
}
