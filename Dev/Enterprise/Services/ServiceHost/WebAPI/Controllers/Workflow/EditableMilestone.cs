using System;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost
{
	public class EditableMilestone : IEditableMilestone
	{
		[JsonProperty("id")]
		public Guid PK { get; set; }
		[JsonProperty("scheduledDate")]
		public DateTimeOffset? ScheduledDate { get; set; }
		[JsonProperty("actualDate")]
		public DateTimeOffset? ActualDate { get; set; }
		[JsonProperty("eventCode")]
		public string EventCode { get; set; }
		[JsonProperty("newScheduledDate")]
		public DateTimeOffset? NewScheduledDate { get; set; }
		[JsonProperty("newActualDate")]
		public DateTimeOffset? NewActualDate { get; set; }
	}
}
