using System.Runtime.Serialization;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	[DataContract]
	public class ValidationResponseData
	{
		[DataMember(Name = "validationResults")]
		public ValidationResults ValidationResults { get; set; }

		[DataMember(Name = "reportingStatus")]
		public string ReportingStatus { get; set; }

		[DataMember(Name = "clearanceStatus")]
		public string ClearanceStatus { get; set; }

		[DataMember(Name = "qrSellertStatus")]
		public string QRSellertStatus { get; set; }

		[DataMember(Name = "qrBuyertStatus")]
		public string QRBuyertStatus { get; set; }
	}
}
