using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public class eHubOutboxMessageArchive
	{
		[Key]
		public Guid OI_PK { get; set; }
		public Guid OI_CC_Sender { get; set; }
		public Guid OI_CC_Recipient { get; set; }
		public Guid? OI_EI_InboxPK { get; set; }
		public string OI_EnvelopeTrackingID { get; set; }
		public string OI_MessageTrackingID { get; set; }
		public string OI_BatchEnvelopeTrackingID { get; set; }
		public string OI_OverrideFilename { get; set; }
		public string OI_OverrideEmailSubject { get; set; }
		public byte OI_Status { get; set; }
		public Guid? OI_DT_Target { get; set; }
		public DateTime OI_InsertUTC { get; set; }
		public DateTime? OI_LastUpdateUTC { get; set; }
		public long? OI_UncompressedLength { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int OI_CompressedLength { get; set; }
		public long OI_SN { get; set; }
		public string OI_Content { get; set; }
		[Column(TypeName = "xml")]
		public string OI_XmlContent { get; set; }

		[ForeignKey("OI_CC_Sender")]
		public virtual eHubClient eHubClient_Sender { get; set; }
		[ForeignKey("OI_CC_Recipient")]
		public virtual eHubClient eHubClient_Recipient { get; set; }
		[ForeignKey("OI_DT_Target")]
		public virtual eHubMessageType eHubMessageType { get; set; }
		[ForeignKey("OI_EI_InboxPK")]
		public virtual eHubInboxMessageArchive eHubInboxMessageArchive { get; set; }
	}
}
