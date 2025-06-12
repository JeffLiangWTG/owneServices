using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubInterfaceCounter
    {
        public Guid CT_TS_PK { get; set; }
        public string CT_Name { get; set; }
        public long CT_Value { get; set; }
        public DateTime CT_LastUpdateUTC { get; set; }
        public bool? CT_Cleanup { get; set; }
    }
}
