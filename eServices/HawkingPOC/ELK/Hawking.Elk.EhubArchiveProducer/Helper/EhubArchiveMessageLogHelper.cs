using System;
using System.Collections.Generic;
using System.Linq;
using Hawking.Elk.EhubArchiveMessages.Model;
using Hawking.Elk.EhubArchiveProducer.Config;

namespace Hawking.Elk.EhubArchiveProducer.Helper
{
    internal static class EhubArchiveMessageLogHelper
    {
        static IDictionary<string, string> EventHeaders { get; set; }

        static EhubArchiveMessageLogHelper()
        {
            EventHeaders = new Dictionary<string, string>
            {
                { "StationId", EhubArchiveMessageLoadingConfig.StationId },
                { "ProcessId", EhubArchiveMessageLoadingConfig.ProcessId },
                { "DataSource", EhubArchiveMessageLoadingConfig.DataSource }
            };
        }

        internal static MessageEvent CreateEhubArchiveMessageEvent(EhubArchiveMessage msg, IEnumerable<EhubClient> ehubClients)
        {
            return new MessageEvent
            {
                EventHeaders = EventHeaders,
                Message = new EhubArchiveMessageLog
                {
                    TrackingId = msg.TrackingId.ToString(),
                    EI_PK = msg.EI_PK.ToString(),
                    ApplicationCode = msg.ApplicationCode,

                    SenderInbox = LookupClient(msg.CC_SenderInbox, ehubClients),
                    RecipientInbox = LookupClient(msg.CC_RecipientInbox, ehubClients),
                    InboxMessageTrackingID = msg.InboxMessageTrackingID.ToString(),
                    ReceivedFromSenderUTC = msg.ReceivedFromSenderUTC ?? new DateTime(),
                    SentToRecipientUTC = msg.SentToRecipientUTC ?? new DateTime(),
                    SenderOutbox = LookupClient(msg.CC_SenderOutbox, ehubClients),
                    RecipientOutbox = LookupClient(msg.CC_RecipientOutbox, ehubClients),
                    OutboxMessageTrackingID = msg.OutboxMessageTrackingID.ToString(),
                    DT_SenderMessageType = msg.DT_SenderMessageType.ToString(),
                    DT_RecipientMessageType = msg.DT_RecipientMessageType.ToString(),
                    ArchivedUTC = msg.ArchivedUTC,
                    ErrorMessage = msg.ErrorMessage,
                    Status = msg.Status ?? -1,
                    EmailSubjectOverride = msg.EmailSubjectOverride,
                    InboxFileNameOverride = msg.InboxFileNameOverride,
                    BillingElementCount = msg.BillingElementCount ?? -1,
                    TS = msg.TS.ToString(),
                    OutboxFileNameOverride = msg.OutboxFileNameOverride,
                    BatchEnvelopeTrackingID = msg.BatchEnvelopeTrackingID.ToString(),
                    SequenceNo = 0,
                    SequenceStart = 0,
                    SequenceEnd = 0
                }
            };
        }

        static string LookupClient(Guid? cc_PK, IEnumerable<EhubClient> ehubClients)
        {
            return cc_PK.HasValue ? ehubClients.First(x => x.TrackingId == cc_PK).CC_ID : string.Empty;
        }

        static string ToString(this Guid? guid)
        {
            return guid.HasValue ? guid.ToString() : Guid.Empty.ToString();
        }

        static string ToString(this DateTime? dateTimeUtc)
        {
            return dateTimeUtc.HasValue ? dateTimeUtc.Value.ToString("u") : string.Empty;
        }
    }
}
