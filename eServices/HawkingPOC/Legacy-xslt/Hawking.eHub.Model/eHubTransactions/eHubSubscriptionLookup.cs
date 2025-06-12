using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubSubscriptionLookup
    {
        public Guid SL_PK { get; set; }
        public Guid SL_ST { get; set; }
        public Guid SL_DT { get; set; }
        public string SL_ValueXpath { get; set; }
        public string SL_ValueProperty { get; set; }

        public eHubMessageType SL_DTNavigation { get; set; }
        public eHubSubscriptionType SL_STNavigation { get; set; }
    }
}
