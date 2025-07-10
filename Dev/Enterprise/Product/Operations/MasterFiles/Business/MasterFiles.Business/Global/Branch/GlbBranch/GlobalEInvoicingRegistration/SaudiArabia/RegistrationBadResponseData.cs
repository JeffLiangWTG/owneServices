using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	[DataContract]
	public class RegistrationBadResponseData
	{
		[DataMember(Name = "code")]
		public string Code { get; set; }

		[DataMember(Name = "message")]
		public string Message { get; set; }
	}

	public class RegistrationBadResponseDataList
	{
		public List<RegistrationBadResponseData> Errors { get; set; }
	}
}
