using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubClientBatching
    {
        public Guid CB_CC_Recipient { get; set; }
        public Guid CB_CC_BatchRecipient { get; set; }
        public string CB_EnvelopeStartTag { get; set; }
        public string CB_EnvelopeEndTag { get; set; }
        public int? CB_MaxCount { get; set; }
        public long? CB_MaxSize { get; set; }
        public TimeSpan? CB_ReleaseScheduleInterval { get; set; }
        public TimeSpan? CB_ReleaseScheduleTimeOfDay { get; set; }
        public bool? CB_ReleaseScheduleSunday { get; set; }
        public bool? CB_ReleaseScheduleMonday { get; set; }
        public bool? CB_ReleaseScheduleTuesday { get; set; }
        public bool? CB_ReleaseScheduleWednesday { get; set; }
        public bool? CB_ReleaseScheduleThursday { get; set; }
        public bool? CB_ReleaseScheduleFriday { get; set; }
        public bool? CB_ReleaseScheduleSaturday { get; set; }
        public DateTime? CB_NextActivationUTC { get; set; }

        public eHubClient CB_CC_RecipientNavigation { get; set; }
    }
}
