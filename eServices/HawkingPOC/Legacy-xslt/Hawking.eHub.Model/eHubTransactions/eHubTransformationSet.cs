using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubTransformationSet
    {
        public eHubTransformationSet()
        {
            eHubCodeSet = new HashSet<eHubCodeSet>();
            eHubTransformationMapping = new HashSet<eHubTransformationMapping>();
        }

        public Guid TS_PK { get; set; }
        public string TS_Name { get; set; }
        public Guid? TS_CC_Sender { get; set; }
        public Guid? TS_CC_Recipient { get; set; }
        public Guid? TS_DT_Source { get; set; }
        public string TS_XPathPredicate { get; set; }
        public string TS_BillingInterfaceName { get; set; }
        public string TS_BillingElement { get; set; }
        public string TS_BillingXPathSource { get; set; }
        public string TS_BillingXPathTarget { get; set; }
        public bool? TS_BillSender { get; set; }
        public bool? TS_BillRecipient { get; set; }
        public Guid? TS_CC_BillOther { get; set; }
        public int? TS_BillingNumMessagesIncluded { get; set; }
        public decimal? TS_BillingFee { get; set; }

        public eHubClient TS_CC_BillOtherNavigation { get; set; }
        public eHubClient TS_CC_RecipientNavigation { get; set; }
        public eHubClient TS_CC_SenderNavigation { get; set; }
        public eHubMessageType TS_DT_SourceNavigation { get; set; }
        public ICollection<eHubCodeSet> eHubCodeSet { get; set; }
        public ICollection<eHubTransformationMapping> eHubTransformationMapping { get; set; }
    }
}
