using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch.Sql;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common.Tests
{
	public class TestQueryResponse
	{
		[JsonInclude, JsonPropertyName("columns")]
		public Column[] Columns { get; set; }

		[JsonInclude, JsonPropertyName("rows")]
		public object[][] Rows { get; set; }
	}
}
