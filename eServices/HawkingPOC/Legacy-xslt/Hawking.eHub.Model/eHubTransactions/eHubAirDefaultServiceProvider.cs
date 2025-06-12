using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubAirDefaultServiceProvider
    {
        public Guid AD_CC_Client { get; set; }
        public Guid AD_DT_MessageType { get; set; }
        public Guid AD_CC_AirServiceProvider { get; set; }

        public eHubClient AD_CC_AirServiceProviderNavigation { get; set; }
        public eHubClient AD_CC_ClientNavigation { get; set; }
        public eHubMessageType AD_DT_MessageTypeNavigation { get; set; }
    }
}
