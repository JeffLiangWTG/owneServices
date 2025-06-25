using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CargoWise.Billing.Kafka.API
{
	public class BillingTransactionForJSON
	{
		[DefaultValue(0)]
		public int BillableCount { get; set; }
		[DefaultValue("")]
		public string Branch { get; set; }
		[DefaultValue("")]
		public string Category { get; set; }
		[DefaultValue("")]
		public string ClientID { get; set; }
		[DefaultValue("")]
		public string ClientNumber { get; set; }
		[DefaultValue("")]
		public string ClientStaffCode { get; set; }
		[DefaultValue("")]
		public string PriceItemCode { get; set; }
		[DefaultValue("")]
		public string Reference1 { get; set; }
		[DefaultValue("")]
		public string Reference2 { get; set; }
		[DefaultValue("")]
		public string Reference3 { get; set; }
		[DefaultValue("")]
		public string Reference4 { get; set; }
		[DefaultValue("")]
		public string Reference5 { get; set; }
		[DefaultValue("")]
		public string ReportingSource { get; set; }
		[JsonProperty("ServiceOccured")]
		public DateTime ServiceOccuredUTC { get; set; }
		[DefaultValue(0)]
		public int Version { get; set; }
		[DefaultValue("")]
		public string MessageTrackingID { get; set; }
		[JsonIgnore]
		public string AdditionalRefs { get; set; }
	}
}
