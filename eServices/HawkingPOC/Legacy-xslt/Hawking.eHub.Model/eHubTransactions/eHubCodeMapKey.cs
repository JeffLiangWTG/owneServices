using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubCodeMapKey
    {
        public eHubCodeMapKey()
        {
            eHubCodeMapValue = new HashSet<eHubCodeMapValue>();
        }

        public Guid CK_PK { get; set; }
        public Guid CK_CS { get; set; }
        public int CK_Order { get; set; }
        public string CK_Key1Value { get; set; }
        public string CK_Key2Value { get; set; }
        public string CK_Key3Value { get; set; }
        public string CK_Key4Value { get; set; }
        public string CK_Key5Value { get; set; }

        public eHubCodeSet CK_CSNavigation { get; set; }
        public ICollection<eHubCodeMapValue> eHubCodeMapValue { get; set; }
    }
}
