using System.Text.Json.Serialization;

namespace Enterprise.Freight.Forwarding.Logging
{
	class PerformanceInfo
	{
		[JsonPropertyName("@timestamp")]
		public DateTime Timestamp { get; set; }
		[JsonPropertyName("srdh.domain")]
		public string Domain { get; set; } = string.Empty;
		[JsonPropertyName("srdh.host")]
		public string Host { get; set; } = string.Empty;
		[JsonPropertyName("srdh.pid")]
		public int Pid { get; set; }
		[JsonPropertyName("srdh.server")]
		public string Server { get; set; } = string.Empty;
		[JsonPropertyName("srdh.database")]
		public string Database { get; set; } = string.Empty;
		[JsonPropertyName("srdh.code")]
		public string Code { get; set; } = string.Empty;
		[JsonPropertyName("srdh.object")]
		public string Object { get; set; } = string.Empty;
		[JsonPropertyName("srdh.pk")]
		public string Pk { get; set; } = string.Empty;
		[JsonPropertyName("srdh.records")]
		public int Records { get; set; }
		[JsonPropertyName("srdh.duration")]
		public int Duration { get; set; }
		[JsonPropertyName("srdh.op")]
		public string Op { get; set; } = string.Empty;
		[JsonPropertyName("srdh.version")]
		public Version Version { get; set; } = new Version();
		[JsonPropertyName("srdh.message")]
		public string Message { get; set; } = string.Empty;
	}
}
