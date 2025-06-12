using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline
{
	class eHubArchiveMessage
	{
		public eHubArchiveMessage()
		{
			AM_PK = Guid.NewGuid();
		}

		public Guid AM_PK { get; set; }
		public string AM_ApplicationCode { get; set; }
		public Guid? AM_CC_SenderInbox { get; set; }
		public Guid? AM_CC_RecipientOutbox { get; set; }
		public Guid? AM_DT_SenderMessageType { get; set; }
		public Guid? AM_DT_RecipientMessageType { get; set; }
		public string AM_SenderMessageRaw { get; set; }
		public string AM_SenderMessageXML { get; set; }
		public string AM_RecipientMessageRaw { get; set; }
		public string AM_RecipientMessageXML { get; set; }
		public DateTime? AM_ReceivedFromSenderUTC { get; set; }
		public DateTime? AM_SentToRecipientUTC { get; set; }
		public DateTime AM_ArchivedUTC { get; set; }
		public string AM_ErrorMessage { get; set; }
		public byte? AM_Status { get; set; }
		public Guid? AM_OutboxMessageTrackingID { get; set; }
		public string AM_EmailSubjectOverride { get; set; }
		public string AM_InboxFileNameOverride { get; set; }
		public long? AM_SenderMessageUncompressedLength { get; set; }
		public long? AM_RecipientMessageUncompressedLength { get; set; }
		public int? AM_BillingElementCount { get; set; }
		public Guid? AM_TS { get; set; }
		public string AM_OutboxFileNameOverride { get; set; }
		public DateTime? AM_ReadyForDeliveryUTC { get; set; }
		public Guid? AM_EI_PK { get; set; }
		public Guid? AM_InboxMessageTrackingID { get; set; }
		public Guid? AM_BatchEnvelopeTrackingID { get; set; }
		public Guid? AM_CC_RecipientInbox { get; set; }
		public Guid? AM_CC_SenderOutbox { get; set; }
	}
}