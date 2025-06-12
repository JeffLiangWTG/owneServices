using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubUSCustomsRegistry
    {
        public Guid ER_PK { get; set; }
        public Guid? ER_CC_Client { get; set; }
        public string ER_ApplicationCode { get; set; }
        public string ER_Name { get; set; }
        public string ER_Value { get; set; }
        public bool? ER_IsProduction { get; set; }

        public eHubClient ER_CC_ClientNavigation { get; set; }
    }
}
