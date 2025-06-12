using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubReferenceFileCache
    {
        public Guid RC_PK { get; set; }
        public Guid RC_RF { get; set; }
        public DateTime RC_ReceivedUTC { get; set; }
        public string RC_Data { get; set; }

        public eHubReferenceFileQuery RC_RFNavigation { get; set; }
    }
}
