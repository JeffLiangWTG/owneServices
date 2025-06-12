using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubSubscriptionType
    {
        public eHubSubscriptionType()
        {
            eHubSubscriptionAutoSubscribe = new HashSet<eHubSubscriptionAutoSubscribe>();
            eHubSubscriptionBroadcaster = new HashSet<eHubSubscriptionBroadcaster>();
            eHubSubscriptionLookup = new HashSet<eHubSubscriptionLookup>();
            eHubSubscriptionValue = new HashSet<eHubSubscriptionValue>();
        }

        public Guid ST_PK { get; set; }
        public string ST_ID { get; set; }
        public string ST_Name { get; set; }
        public int? ST_ExpiryDays { get; set; }

        public ICollection<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribe { get; set; }
        public ICollection<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcaster { get; set; }
        public ICollection<eHubSubscriptionLookup> eHubSubscriptionLookup { get; set; }
        public ICollection<eHubSubscriptionValue> eHubSubscriptionValue { get; set; }
    }
}
