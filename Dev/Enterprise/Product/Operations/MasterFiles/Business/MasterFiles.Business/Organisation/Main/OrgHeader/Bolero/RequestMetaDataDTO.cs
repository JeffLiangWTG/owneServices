using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business
{
	public class RequestMetaDataDTO
	{
		[JsonProperty("overrideFlag")]
		public bool OverrideFlag { get; set; }
	}
}
