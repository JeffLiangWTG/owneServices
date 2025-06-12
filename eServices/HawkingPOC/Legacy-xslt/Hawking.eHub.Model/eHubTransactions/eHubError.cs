using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubError
    {
        public Guid EE_PK { get; set; }
        public DateTime EE_DateTimeUTC { get; set; }
        public string EE_Source { get; set; }
        public string EE_ErrorType { get; set; }
        public string EE_Description { get; set; }
        public string EE_ErrorDetail { get; set; }
        public Guid? EE_EI_Inbox { get; set; }
        public Guid? EE_OI_Outbox { get; set; }
        public bool? EE_Alerted { get; set; }
    }
}
