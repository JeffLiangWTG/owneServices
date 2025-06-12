using System.Text.Json.Serialization;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WGReport
{
	public class WGRUsageTransaction
	{
#pragma warning disable CS8618
		public string Id { get; set; }
		public int UsageCount { get; set; }
		public string EnterpriseCode { get; set; }
		public string ServerCode { get; set; }
		public string Environment { get; set; }
		public string CompanyCode { get; set; }
		public string CompanyName { get; set; }
		public string BranchCode { get; set; }
		public string UsageCode { get; set; }
		public string Category { get; set; }
		public string Reference1 { get; set; }
		public string Reference2 { get; set; }
		public string Reference3 { get; set; }
		public string Reference4 { get; set; }
		public string Reference5 { get; set; }
		public string ReportingSource { get; set; }
		[JsonPropertyName("@timestamp")]
		public DateTime ServiceOccuredUTC { get; set; }
		public DateTime SubmitToELKTime { get; set; }
#pragma warning restore CS8618
	}
}
