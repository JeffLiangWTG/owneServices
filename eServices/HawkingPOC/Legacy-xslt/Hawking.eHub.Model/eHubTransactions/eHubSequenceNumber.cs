using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubSequenceNumber
    {
        public Guid SN_CC_Sender { get; set; }
        public Guid SN_CC_Recipient { get; set; }
        public long SN_GeneratedSequence { get; set; }
        public long SN_DeliveredSequence { get; set; }
    }
}
