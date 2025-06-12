using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubInboxXmlContent
    {
        public Guid EX_EI_Inbox { get; set; }
        public string EX_XmlContent { get; set; }
        public Guid? EX_DT_Source { get; set; }
        public long? EX_UncompressedLength { get; set; }

        public eHubMessageType EX_DT_SourceNavigation { get; set; }
    }
}
