using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubSubscriptionAutoSubscribe
    {
        public Guid SA_PK { get; set; }
        public Guid SA_ST { get; set; }
        public Guid? SA_CC_Recipient { get; set; }
        public Guid? SA_DT { get; set; }
        public string SA_ValueXpath { get; set; }
        public string SA_ValueProperty { get; set; }
        public string SA_ReferenceXpath { get; set; }
        public string SA_ReferenceProperty { get; set; }

        public eHubMessageType SA_DTNavigation { get; set; }
        public eHubSubscriptionType SA_STNavigation { get; set; }
    }
}
