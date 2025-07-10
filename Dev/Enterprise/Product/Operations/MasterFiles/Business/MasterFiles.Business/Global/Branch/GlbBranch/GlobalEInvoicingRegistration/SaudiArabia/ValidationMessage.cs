using System.Runtime.Serialization;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	[DataContract]
	public class ValidationMessage
	{
		[DataMember(Name = "type")]
		public string Type { get; set; }

		[DataMember(Name = "code")]
		public string Code { get; set; }

		[DataMember(Name = "category")]
		public string Category { get; set; }

		[DataMember(Name = "message")]
		public string Message { get; set; }

		[DataMember(Name = "status")]
		public string Status { get; set; }
	}
}
