using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common;

public abstract class ElasticSearchPluginBase : AbstractPlugin
{
	private ElasticsearchClient _client;
	public ElasticsearchClient Client => _client ??= CreateElasticClient();
	public override void UpdateSettings(PluginSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);
		ElasticEndpoint = settings.Parameters.FirstOrDefault(x => x.Name.Equals("ElasticEndpoint"))?.Value;
		ApiKey = settings.Parameters.FirstOrDefault(x => x.Name.Equals("ElasticApiKey"))?.Value;
		ArgumentException.ThrowIfNullOrEmpty(ElasticEndpoint);
		ArgumentException.ThrowIfNullOrEmpty(ApiKey);
	}

	protected internal virtual ElasticsearchClient CreateElasticClient()
	{
		var pool = new SingleNodePool(new Uri(ElasticEndpoint));
		var connectionSettings = new ElasticsearchClientSettings(pool, sourceSerializer: (builtin, settings) => new DefaultSourceSerializer(settings, options => {})).DisablePing();
		connectionSettings.Authentication(new ApiKey(EhubServerDecryptor.Decrypt(ApiKey)));
		connectionSettings.DisableDirectStreaming();
		connectionSettings.DefaultFieldNameInferrer(p => p);
		connectionSettings.ThrowExceptions();
		connectionSettings.RequestTimeout(TimeSpan.FromMinutes(30));
		return new ElasticsearchClient(connectionSettings);
	}

	#region Settings
	public string ElasticEndpoint { get; set; }
	public string ApiKey { get; set; }
	#endregion
}
