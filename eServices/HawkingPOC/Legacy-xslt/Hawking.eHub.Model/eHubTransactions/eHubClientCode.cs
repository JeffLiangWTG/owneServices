using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubClientCode
    {
        public Guid CM_PK { get; set; }
        public Guid CM_SC { get; set; }
        public string CM_ClientCode { get; set; }
        public Guid CM_CC { get; set; }

        public eHubClient CM_CCNavigation { get; set; }
    }
}
