using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubRoutingRuleFact
    {
        public Guid RX_PK { get; set; }
        public Guid RX_RR { get; set; }
        public string RX_Name { get; set; }
        public string RX_Type { get; set; }
        public string RX_Query { get; set; }
        public Guid? RX_RR_ComputeRule { get; set; }

        public eHubRoutingRule RX_RRNavigation { get; set; }
        public eHubRoutingRule RX_RR_ComputeRuleNavigation { get; set; }
    }
}
