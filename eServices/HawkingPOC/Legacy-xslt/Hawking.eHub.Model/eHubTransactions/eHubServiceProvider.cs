using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubServiceProvider
    {
        public eHubServiceProvider()
        {
            eHubRoutingRule = new HashSet<eHubRoutingRule>();
            eHubServiceProviderRequiredRegistration = new HashSet<eHubServiceProviderRequiredRegistration>();
        }

        public Guid SP_CC_Service { get; set; }
        public Guid SP_CC_Provider { get; set; }
        public Guid? SP_RR { get; set; }
        public Guid SP_PK { get; set; }

        public eHubClient SP_CC_ProviderNavigation { get; set; }
        public eHubClient SP_CC_ServiceNavigation { get; set; }
        public ICollection<eHubRoutingRule> eHubRoutingRule { get; set; }
        public ICollection<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistration { get; set; }
    }
}
