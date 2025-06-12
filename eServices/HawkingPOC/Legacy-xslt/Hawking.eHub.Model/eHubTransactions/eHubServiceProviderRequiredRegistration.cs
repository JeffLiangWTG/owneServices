using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubServiceProviderRequiredRegistration
    {
        public Guid SX_RT { get; set; }
        public string SX_LookupFactName { get; set; }
        public string SX_QualifierFactName { get; set; }
        public Guid SX_SP { get; set; }

        public eHubRegistrationType SX_RTNavigation { get; set; }
        public eHubServiceProvider SX_SPNavigation { get; set; }
    }
}
