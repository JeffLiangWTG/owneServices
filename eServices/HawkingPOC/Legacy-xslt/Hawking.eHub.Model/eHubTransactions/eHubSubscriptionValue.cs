using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubSubscriptionValue
    {
        public Guid SV_PK { get; set; }
        public Guid SV_ST { get; set; }
        public Guid SV_CC_Sender { get; set; }
        public string SV_Value { get; set; }
        public string SV_Reference { get; set; }
        public DateTime SV_SubscribedUTC { get; set; }
        public DateTime? SV_ExpiryUTC { get; set; }
        public Guid SV_CC_Recipient { get; set; }
        public string SV_ReferenceType { get; set; }

        public eHubClient SV_CC_RecipientNavigation { get; set; }
        public eHubClient SV_CC_SenderNavigation { get; set; }
        public eHubSubscriptionType SV_STNavigation { get; set; }
    }
}
