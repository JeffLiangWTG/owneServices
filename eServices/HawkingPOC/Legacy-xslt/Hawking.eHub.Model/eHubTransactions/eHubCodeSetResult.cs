using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubCodeSetResult
    {
        public eHubCodeSetResult()
        {
            eHubCodeMapValue = new HashSet<eHubCodeMapValue>();
        }

        public Guid CR_PK { get; set; }
        public Guid CR_CS { get; set; }
        public int CR_Order { get; set; }
        public string CR_Name { get; set; }

        public eHubCodeSet CR_CSNavigation { get; set; }
        public ICollection<eHubCodeMapValue> eHubCodeMapValue { get; set; }
    }
}
