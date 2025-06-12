using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubAirConnection
    {
        public eHubAirConnection()
        {
            eHubAirConnectionPerBranch = new HashSet<eHubAirConnectionPerBranch>();
        }

        public Guid AC_CC_Client { get; set; }
        public Guid AC_CC_AirServiceProvider { get; set; }
        public string AC_PIMA { get; set; }
        public string AC_PASSWORD { get; set; }

        public eHubClient AC_CC_AirServiceProviderNavigation { get; set; }
        public eHubClient AC_CC_ClientNavigation { get; set; }
        public ICollection<eHubAirConnectionPerBranch> eHubAirConnectionPerBranch { get; set; }
    }
}
