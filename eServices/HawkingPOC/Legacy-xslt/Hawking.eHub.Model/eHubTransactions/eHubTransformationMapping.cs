using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubTransformationMapping
    {
        public Guid TM_TS_PK { get; set; }
        public byte TM_Order { get; set; }
        public Guid TM_TT_PK { get; set; }

        public eHubTransformationSet TM_TS_PKNavigation { get; set; }
        public eHubTransformationType TM_TT_PKNavigation { get; set; }
    }
}
