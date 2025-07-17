using System.Text.Json.Serialization;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WiseCloudReadOnly;

internal class HaproxyLogEntry
{
    [JsonPropertyName("haproxy.client.ip")]
    public string ClientIp { get; set; } = string.Empty;

    [JsonPropertyName("haproxy.bytes.read")]
    public double BytesRead { get; set; }

    [JsonPropertyName("haproxy.bytes_uploaded")]
    public double BytesUploaded { get; set; }

    [JsonPropertyName("haproxy.server_name")]
    public string ServerName { get; set; } = string.Empty;

    [JsonPropertyName("@timestamp")]
    public DateTime Timestamp { get; set; }
}
