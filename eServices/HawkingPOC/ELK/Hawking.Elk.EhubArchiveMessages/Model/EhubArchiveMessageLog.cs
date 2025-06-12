using System;

namespace Hawking.Elk.EhubArchiveMessages.Model
{
    public class EhubArchiveMessageLog
    {
        public string TrackingId { get; set; }
        public string EI_PK { get; set; }
        public string ApplicationCode { get; set; }

        public string SenderInbox { get; set; }
        public string RecipientInbox { get; set; }
        public string InboxMessageTrackingID { get; set; }

        public DateTime ReceivedFromSenderUTC { get; set; }
        public DateTime SentToRecipientUTC { get; set; }

        public string SenderOutbox { get; set; }
        public string RecipientOutbox { get; set; }
        public string OutboxMessageTrackingID { get; set; }

        public string DT_SenderMessageType { get; set; }
        public string DT_RecipientMessageType { get; set; }

        public DateTime ArchivedUTC { get; set; }
        public string ErrorMessage { get; set; }
        public int Status { get; set; }

        public string EmailSubjectOverride { get; set; }
        public string InboxFileNameOverride { get; set; }

        public int BillingElementCount { get; set; }
        public string TS { get; set; }
        public string OutboxFileNameOverride { get; set; }

        public string BatchEnvelopeTrackingID { get; set; }

        public int SequenceNo { get; set; }
        public int SequenceStart { get; set; }
        public int SequenceEnd { get; set; }
    }
}
