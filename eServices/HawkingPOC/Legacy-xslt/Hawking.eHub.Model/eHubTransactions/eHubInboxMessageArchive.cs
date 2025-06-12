using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubInboxMessageArchive
    {
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
        public string EI_Content { get; set; }
        public DateTime EI_InsertUTC { get; set; }
        public long? EI_SN { get; set; }
    }
}
