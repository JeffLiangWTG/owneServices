using Newtonsoft.Json;

public class HaproxyLogEntry
{
	[JsonProperty("@timestamp")]
	public DateTime Timestamp { get; set; }

	[JsonProperty("haproxy")]
	public HaproxyData Haproxy { get; set; }
}

public class HaproxyData
{
	[JsonProperty("client")]
	public HaproxyClient Client { get; set; }

	[JsonProperty("bytes")]
	public HaproxyBytes Bytes { get; set; }
}

public class HaproxyClient
{
	[JsonProperty("ip")]
	public string Ip { get; set; }
}

public class HaproxyBytes
{
	[JsonProperty("read")]
	public int Read { get; set; }

	[JsonProperty("uploaded")]
	public int Uploaded { get; set; }
}
