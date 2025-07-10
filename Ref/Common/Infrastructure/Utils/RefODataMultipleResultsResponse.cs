using System.Collections.Generic;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class RefODataMultipleResultsResponse<T>
	{
		[JsonProperty("@odata.context")]
		public string Context { get; set; }
		public List<T> Value { get; set; }
	}
}
