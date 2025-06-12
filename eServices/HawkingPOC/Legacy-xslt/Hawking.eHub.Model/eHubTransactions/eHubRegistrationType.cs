using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubRegistrationType
    {
        public eHubRegistrationType()
        {
            eHubClientRegistration = new HashSet<eHubClientRegistration>();
            eHubClientSystemRegistration = new HashSet<eHubClientSystemRegistration>();
            eHubServiceOperatorRegistration = new HashSet<eHubServiceOperatorRegistration>();
            eHubServiceProviderRequiredRegistration = new HashSet<eHubServiceProviderRequiredRegistration>();
        }

        public Guid RT_PK { get; set; }
        public string RT_ID { get; set; }
        public string RT_Description { get; set; }
        public string RT_RegistrantType { get; set; }

        public ICollection<eHubClientRegistration> eHubClientRegistration { get; set; }
        public ICollection<eHubClientSystemRegistration> eHubClientSystemRegistration { get; set; }
        public ICollection<eHubServiceOperatorRegistration> eHubServiceOperatorRegistration { get; set; }
        public ICollection<eHubServiceProviderRequiredRegistration> eHubServiceProviderRequiredRegistration { get; set; }
    }
}
