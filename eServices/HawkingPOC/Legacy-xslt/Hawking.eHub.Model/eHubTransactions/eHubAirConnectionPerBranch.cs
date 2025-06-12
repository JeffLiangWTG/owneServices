using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubAirConnectionPerBranch
    {
        public Guid AB_CC_Client { get; set; }
        public Guid AB_CC_AirServiceProvider { get; set; }
        public string AB_IssuingCarrierAgentIATACode { get; set; }
        public string AB_PIMA { get; set; }
        public string AB_PASSWORD { get; set; }

        public eHubAirConnection AB_CC_ { get; set; }
    }
}
