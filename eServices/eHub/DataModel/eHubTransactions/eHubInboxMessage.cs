using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public class eHubInboxMessage
	{
		[Key]
		public Guid EI_PK { get; set; }
		public string EI_MessageTrackingID { get; set; }
		public string EI_EnvelopeTrackingID { get; set; }
		public Guid EI_CC_Sender { get; set; }
		public Guid? EI_CC_Recipient { get; set; }
		public string EI_MessageType { get; set; }
		public bool EI_IsFlatFile { get; set; }
		public string EI_EmailSubjectOverride { get; set; }
		public string EI_FileNameOverride { get; set; }
		public string EI_ApplicationCode { get; set; }
		public byte EI_Status { get; set; }
		public DateTime? EI_InsertUTC { get; set; }
		public DateTime EI_LastUpdateUTC { get; set; } = DateTime.UtcNow;
		public long? EI_SN { get; set; }
		public string EI_Content { get; set; }
	    [Column(TypeName = "xml")]
        public string EI_ContextProperty { get; set; }

		[ForeignKey("EI_CC_Sender")]
		public virtual eHubClient eHubClient_Sender { get; set; }
		[ForeignKey("EI_CC_Recipient")]
		public virtual eHubClient eHubClient_Recipient { get; set; }
		[InverseProperty("eHubInboxMessage")]
		public virtual List<eHubError> eHubErrors { get; set; }
		[InverseProperty("eHubInboxMessage")]
		public virtual List<eHubOutboxMessage> eHubOutboxMessages { get; set; }

		public eHubInboxMessage()
		{
			eHubErrors = new List<eHubError>();
			eHubOutboxMessages = new List<eHubOutboxMessage>();
		}
	}
}
