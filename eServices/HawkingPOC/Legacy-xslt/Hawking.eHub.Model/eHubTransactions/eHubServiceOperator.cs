using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubServiceOperator
    {
        public eHubServiceOperator()
        {
            eHubServiceOperatorRegistration = new HashSet<eHubServiceOperatorRegistration>();
        }

        public Guid SO_PK { get; set; }
        public string SO_ID { get; set; }
        public string SO_Name { get; set; }

        public ICollection<eHubServiceOperatorRegistration> eHubServiceOperatorRegistration { get; set; }
    }
}
