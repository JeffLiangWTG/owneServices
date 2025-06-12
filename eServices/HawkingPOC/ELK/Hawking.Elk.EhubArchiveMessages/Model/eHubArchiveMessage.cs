using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hawking.Elk.Common.Model;

namespace Hawking.Elk.EhubArchiveMessages.Model
{
    [Table("eHubArchiveMessage")]
    public partial class EhubArchiveMessage : IEntity
    {
        [Key]
        public Guid TrackingId { get; set; }
        public Guid? EI_PK { get; set; }
        public string ApplicationCode { get; set; }

        public Guid? CC_SenderInbox { get; set; }
        public Guid? CC_RecipientInbox { get; set; }
        public Guid? InboxMessageTrackingID { get; set; }

        public DateTime? ReceivedFromSenderUTC { get; set; }
        public DateTime? SentToRecipientUTC { get; set; }

        public Guid? CC_SenderOutbox { get; set; }
        public Guid? CC_RecipientOutbox { get; set; }
        public Guid? OutboxMessageTrackingID { get; set; }

        public Guid? DT_SenderMessageType { get; set; }
        public Guid? DT_RecipientMessageType { get; set; }

        public string SenderMessageRaw { get; set; }
        public string SenderMessageXML { get; set; }

        public string RecipientMessageRaw { get; set; }
        public string RecipientMessageXML { get; set; }

        public DateTime ArchivedUTC { get; set; }
        public string ErrorMessage { get; set; }
        public byte? Status { get; set; }

        public string EmailSubjectOverride { get; set; }
        public string InboxFileNameOverride { get; set; }

        public long? SenderMessageUncompressedLength { get; set; }
        public long? RecipientMessageUncompressedLength { get; set; }

        public int? BillingElementCount { get; set; }
        public Guid? TS { get; set; }
        public string OutboxFileNameOverride { get; set; }
        public DateTime? ReadyForDeliveryUTC { get; set; }

        public Guid? BatchEnvelopeTrackingID { get; set; }
    }
}