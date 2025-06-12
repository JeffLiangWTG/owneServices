using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubAirReferenceNumber
    {
        public Guid AN_CC_Sender { get; set; }
        public Guid AN_CC_AirServiceProvider { get; set; }
        public int AN_ReferenceNumber { get; set; }

        public eHubClient AN_CC_AirServiceProviderNavigation { get; set; }
        public eHubClient AN_CC_SenderNavigation { get; set; }
    }
}
