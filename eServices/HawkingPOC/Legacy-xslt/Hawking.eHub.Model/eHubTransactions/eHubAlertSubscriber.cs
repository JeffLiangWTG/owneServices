using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubAlertSubscriber
    {
        public Guid ES_PK { get; set; }
        public Guid? ES_EA_Alert { get; set; }
        public string ES_Email { get; set; }

        public eHubAlert ES_EA_AlertNavigation { get; set; }
    }
}
