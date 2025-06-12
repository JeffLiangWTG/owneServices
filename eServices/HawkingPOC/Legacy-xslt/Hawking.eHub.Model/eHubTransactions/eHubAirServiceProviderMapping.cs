using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubAirServiceProviderMapping
    {
        public Guid AM_CC_Client { get; set; }
        public Guid AM_CC_Airline { get; set; }
        public Guid AM_DT_MessageType { get; set; }
        public Guid AM_CC_AirServiceProvider { get; set; }

        public eHubClient AM_CC_AirServiceProviderNavigation { get; set; }
        public eHubClient AM_CC_AirlineNavigation { get; set; }
        public eHubClient AM_CC_ClientNavigation { get; set; }
        public eHubMessageType AM_DT_MessageTypeNavigation { get; set; }
    }
}
