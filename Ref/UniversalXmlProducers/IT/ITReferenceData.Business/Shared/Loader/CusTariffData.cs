using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
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
