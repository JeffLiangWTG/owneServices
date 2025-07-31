using Newtonsoft.Json;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;

public class HaproxyLogEntry
{
    [JsonProperty("@timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("haproxy")]
    public HaproxyData Haproxy { get; set; } = new();
}

public class HaproxyData
{
    [JsonProperty("client")]
    public HaproxyClient Client { get; set; } = new();

    [JsonProperty("bytes")]
    public HaproxyBytes Bytes { get; set; } = new();
}

public class HaproxyClient
{
    [JsonProperty("ip")]
    public string Ip { get; set; } = string.Empty;
}

public class HaproxyBytes
{
    [JsonProperty("read")]
    public int Read { get; set; }

    [JsonProperty("uploaded")]
    public int Uploaded { get; set; }
}
