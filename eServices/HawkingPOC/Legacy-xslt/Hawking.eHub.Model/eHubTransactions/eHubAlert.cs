using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubAlert
    {
        public eHubAlert()
        {
            eHubAlertSubscriber = new HashSet<eHubAlertSubscriber>();
        }

        public Guid EA_PK { get; set; }
        public Guid? EA_CC_Sender { get; set; }
        public Guid? EA_CC_Recipient { get; set; }
        public string EA_Source { get; set; }
        public string EA_ErrorType { get; set; }

        public eHubClient EA_CC_RecipientNavigation { get; set; }
        public eHubClient EA_CC_SenderNavigation { get; set; }
        public ICollection<eHubAlertSubscriber> eHubAlertSubscriber { get; set; }
    }
}
