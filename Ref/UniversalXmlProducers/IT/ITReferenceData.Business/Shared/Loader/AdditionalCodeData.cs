using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public class AdditionalCodeDataWrapper
	{
		[JsonProperty("value")]
		public AdditionalCodeData[] Value { get; set; }
	}

	public class AdditionalCodeData
	{
		[JsonProperty("ZZD_Code")]
		public string Code { get; set; }

		[JsonProperty("ZZD_Description")]
		public string Description { get; set; }
	}
}
