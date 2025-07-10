using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader
{
	public class CusTariffDataWrapper
	{
		[JsonProperty("value")]
		public CusTariffData[] Value { get; set; }
	}

	public class CusTariffData
	{
		[JsonProperty("ZZ1_TariffCode")]
		public string TariffCode { get; set; }
	}
}
