using System.Runtime.Serialization;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	[DataContract]
	public class RegistrationGoodResponseData
	{
		[DataMember(Name = "requestID")]
		public string RequestID { get; set; }

		[DataMember(Name = "dispositionMessage")]
		public string DispositionMessage { get; set; }

		[DataMember(Name = "binarySecurityToken")]
		public string BinarySecurityToken { get; set; }

		[DataMember(Name = "secret")]
		public string Secret { get; set; }

		[DataMember(Name = "errors")]
		public string Errors { get; set; }

		[DataMember(Name = "tokenType")]
		public string TokenType { get; set; }
	}
}
