using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubMessageReferenceRegistry
    {
        public Guid CR_PK { get; set; }
        public Guid CR_CC_Client { get; set; }
        public string CR_ApplicationCode { get; set; }
        public string CR_MessageReference { get; set; }
        public string CR_Password { get; set; }

        public eHubClient CR_CC_ClientNavigation { get; set; }
    }
}
