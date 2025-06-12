using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubServiceOperatorRegistration
    {
        public Guid SR_SO { get; set; }
        public Guid SR_RT { get; set; }
        public string SR_Code { get; set; }

        public eHubRegistrationType SR_RTNavigation { get; set; }
        public eHubServiceOperator SR_SONavigation { get; set; }
    }
}
