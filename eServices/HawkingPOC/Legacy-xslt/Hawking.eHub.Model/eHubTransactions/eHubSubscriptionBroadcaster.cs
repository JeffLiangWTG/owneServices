using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubSubscriptionBroadcaster
    {
        public Guid SB_PK { get; set; }
        public Guid SB_ST { get; set; }
        public Guid SB_CC { get; set; }

        public eHubClient SB_CCNavigation { get; set; }
        public eHubSubscriptionType SB_STNavigation { get; set; }
    }
}
