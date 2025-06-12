using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubOutboxMessageArchive
    {
        public Guid OI_PK { get; set; }
        public Guid OI_CC_Sender { get; set; }
        public Guid OI_CC_Recipient { get; set; }
        public string OI_EI_InboxPK { get; set; }
        public string OI_EnvelopeTrackingID { get; set; }
        public string OI_MessageTrackingID { get; set; }
        public string OI_BatchEnvelopeTrackingID { get; set; }
        public string OI_OverrideFilename { get; set; }
        public string OI_OverrideEmailSubject { get; set; }
        public byte OI_Status { get; set; }
        public string OI_Content { get; set; }
        public string OI_XmlContent { get; set; }
        public Guid? OI_DT_Target { get; set; }
        public DateTime OI_InsertUTC { get; set; }
        public DateTime? OI_LastUpdateUTC { get; set; }
        public long? OI_UncompressedLength { get; set; }
        public long? OI_CompressedLength { get; set; }
        public long? OI_SN { get; set; }
    }
}
