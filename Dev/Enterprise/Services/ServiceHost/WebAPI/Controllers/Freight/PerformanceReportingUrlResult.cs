using System.Collections.Generic;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost
{
	public class PerformanceReportingUrlResult
	{
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("errors")]
		public IEnumerable<string> Errors { get; set; }
	}
}
