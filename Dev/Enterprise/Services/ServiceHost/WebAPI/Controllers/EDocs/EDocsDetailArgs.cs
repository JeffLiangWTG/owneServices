using System;
using System.Runtime.Serialization;

namespace Enterprise.Services.ServiceHost
{
	[DataContract]
	public class EDocsDetailArgs
	{
		[DataMember(Name = "description")]
		public string Description { get; set; }

		[DataMember(Name = "isPublished")]
		public bool IsPublished { get; set; }

		[DataMember(Name = "docType")]
		public Guid RefDocTypePK { get; set; }
	}
}