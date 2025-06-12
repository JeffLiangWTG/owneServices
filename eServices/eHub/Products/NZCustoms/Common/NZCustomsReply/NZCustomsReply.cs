using System.Runtime.Serialization;

namespace CargoWise.eHub.Products.NZCustoms.Common
{
	[DataContract(Namespace = "http://cargowise.com/ehub/products/nzcustomsreply", Name = "NZCustomsReply")]
	public class NZCustomsReply
	{
		[DataMember]
		public string Reference { get; set; }

		[DataMember]
		public string Content { get; set; }

		[DataMember]
		public Attachment[] Attachments { get; set; }
	}
}