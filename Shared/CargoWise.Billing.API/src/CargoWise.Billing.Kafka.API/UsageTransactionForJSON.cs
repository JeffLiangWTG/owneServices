using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CargoWise.Billing.Kafka.API
{
	public class UsageTransactionForJSON
	{
		[DefaultValue(0)]
		public int UsageCount { get; set; }
		[JsonProperty("ServiceOccured")]
		public DateTime ServiceOccuredUTC { get; set; }
		[JsonIgnore]
		public string AdditionalRefs { get; set; }
		public string EnterpriseCode { get; set; }
		public string ServerCode { get; set; }
		public string Environment { get; set; }
		public string CompanyCode { get; set; }
		public string CompanyName { get; set; }
		public string BranchCode { get; set; }
		public string UsageCode { get; set; }
	}
}
