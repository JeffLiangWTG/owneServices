using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubClientSchedule
    {
        public Guid CS_PK { get; set; }
        public Guid CS_CC_Recipient { get; set; }
        public DateTime CS_StartTime { get; set; }
        public byte CS_Recurrence { get; set; }
        public DateTime CS_NextActivation { get; set; }

        public eHubClient CS_CC_RecipientNavigation { get; set; }
    }
}
