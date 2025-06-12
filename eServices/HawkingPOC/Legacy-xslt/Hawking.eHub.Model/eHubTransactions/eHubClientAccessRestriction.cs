using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubClientAccessRestriction
    {
        public Guid CP_PK { get; set; }
        public Guid CP_Client { get; set; }
        public string CP_Restriction { get; set; }

        public eHubClient CP_ClientNavigation { get; set; }
    }
}
