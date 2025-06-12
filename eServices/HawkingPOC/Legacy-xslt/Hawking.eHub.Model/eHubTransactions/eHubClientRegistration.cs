using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubClientRegistration
    {
        public Guid CX_PK { get; set; }
        public Guid CX_CC { get; set; }
        public Guid CX_RT { get; set; }
        public string CX_Qualifier { get; set; }
        public string CX_Code { get; set; }
        public string CX_Attr1 { get; set; }
        public string CX_Password1 { get; set; }
        public byte? CX_Flag1 { get; set; }
        public byte? CX_Flag2 { get; set; }

        public eHubClient CX_CCNavigation { get; set; }
        public eHubRegistrationType CX_RTNavigation { get; set; }
    }
}
