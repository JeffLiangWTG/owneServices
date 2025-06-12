using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	[Serializable]
	public partial class eHubCertificate : EFSerializable
	{
		[Key]
		public virtual Guid CE_PK { get; set; }
		public virtual string CE_Category { get; set; }
		public virtual Guid? CE_EH_Owner { get; set; }
		public virtual string CE_ID { get; set; }
		public virtual Guid? CE_CC_Owner { get; set; }
		public virtual DateTime CE_AddedUTC { get; set; }
		public virtual string CE_ContainerType { get; set; }
		public virtual byte[] CE_BinaryContainer { get; set; }
		public virtual string CE_TextContainer { get; set; }
		public virtual string CE_Password { get; set; }
		public virtual DateTime? CE_ValidFromUTC { get; set; }
		public virtual DateTime? CE_ValidToUTC { get; set; }
		public virtual DateTime? CE_ActiveFromUTC { get; set; }
		public virtual string CE_Thumbprint { get; set; }
		public virtual string CE_Issuer { get; set; }
		public virtual string CE_SerialNumber { get; set; }
		public virtual string CE_SubjectKeyIdentifier { get; set; }
		public virtual byte? CE_Flag1 { get; set; }

		[ForeignKey("CE_CC_Owner"), IgnoreDataMember]
		public virtual eHubClient eHubClient { get; set; }

		[ForeignKey("CE_EH_Owner"), IgnoreDataMember]
		public virtual eHubClientSystem eHubClientSystem { get; set; }

		public eHubCertificate()
		{
			CE_PK = Guid.NewGuid();
		}

		protected eHubCertificate(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{}
	}
}
