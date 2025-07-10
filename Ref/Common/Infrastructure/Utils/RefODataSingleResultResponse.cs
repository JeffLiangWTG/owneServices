using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class RefODataSingleResultResponse<T>
	{
		[JsonProperty("@odata.context")]
		public string Context { get; set; }
		public T Value { get; set; }
	}
}
