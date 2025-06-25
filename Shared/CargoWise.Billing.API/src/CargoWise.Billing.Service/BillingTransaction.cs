using System;
using System.Runtime.Serialization;

namespace CargoWise.Billing.Service
{
	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/CargoWise.eServices.Billing.WcfService")]
	public sealed class BillingTransaction
	{
		[DataMember(Order = 2)]
		public int Version { get; set; }

		[DataMember(Order = 2)]
		public string Category { get; set; }

		[DataMember]
		public string PriceItemCode { get; set; }

		[DataMember]
		public int BillableCount { get; set; }

		[DataMember]
		public string ReportingSource { get; set; }

		[DataMember]
		public DateTime ServiceOccuredUTC { get; set; }

		[DataMember]
		public string ClientID { get; set; }

		[DataMember]
		public string ClientNumber { get; set; }

		[DataMember]
		public string ClientStaffCode { get; set; }

		[DataMember(Order = 2)]
		public string Branch { get; set; }

		[DataMember]
		public string Reference1 { get; set; }

		[DataMember]
		public string Reference2 { get; set; }

		[DataMember]
		public string Reference3 { get; set; }

		[DataMember]
		public string Reference4 { get; set; }

		[DataMember(Order = 2)]
		public string Reference5 { get; set; }

		[DataMember(Order = 3)]
		public string MessageTrackingID { get; set; }

		[DataMember]
		public string AdditionalRefs { get; set; }
	}
}
