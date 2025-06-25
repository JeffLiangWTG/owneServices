using System;
using System.Runtime.Serialization;

namespace CargoWise.Billing.Service
{
	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/CargoWise.eServices.Billing.WcfService")]
	public sealed class UsageTransaction
	{
		[DataMember]
		public int UsageCount { get; set; }
		[DataMember]
		public DateTime ServiceOccuredUTC { get; set; }
		[DataMember]
		public string AdditionalRefs { get; set; }
		[DataMember]
		public string EnterpriseCode { get; set; }
		[DataMember]
		public string ServerCode { get; set; }
		[DataMember]
		public string Environment { get; set; }
		[DataMember]
		public string CompanyCode { get; set; }
		[DataMember]
		public string CompanyName { get; set; }
		[DataMember]
		public string BranchCode { get; set; }
		[DataMember]
		public string UsageCode { get; set; }
	}
}
