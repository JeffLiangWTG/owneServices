using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	[DataContract]
	public class ValidationResults
	{
		[DataMember(Name = "infoMessages")]
		public List<ValidationMessage> InfoMessages { get; set; }
		[DataMember(Name = "warningMessages")]
		public List<ValidationMessage> WarningMessages { get; set; }
		[DataMember(Name = "errorMessages")]
		public List<ValidationMessage> ErrorMessages { get; set; }
		[DataMember(Name = "status")]
		public string Status { get; set; }
	}
}
