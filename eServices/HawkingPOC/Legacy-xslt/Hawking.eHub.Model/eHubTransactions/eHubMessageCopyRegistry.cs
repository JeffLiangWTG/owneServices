using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubMessageCopyRegistry
    {
        public Guid MC_CC_Sender { get; set; }
        public Guid MC_CC_Recipient { get; set; }
        public string MC_InboxLikePattern { get; set; }
        public string MC_OutboxMessageXpath { get; set; }
        public Guid MC_CC_OriginalMessage_Copy_Recipient { get; set; }
        public string MC_OutboxLikePattern { get; set; }

        public eHubClient MC_CC_OriginalMessage_Copy_RecipientNavigation { get; set; }
        public eHubClient MC_CC_RecipientNavigation { get; set; }
        public eHubClient MC_CC_SenderNavigation { get; set; }
    }
}
