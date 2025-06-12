using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubCodeSet
    {
        public eHubCodeSet()
        {
            eHubCodeMapKey = new HashSet<eHubCodeMapKey>();
            eHubCodeSetResult = new HashSet<eHubCodeSetResult>();
        }

        public Guid CS_PK { get; set; }
        public string CS_Name { get; set; }
        public Guid? CS_TS { get; set; }
        public Guid CS_CC_Sender { get; set; }
        public Guid CS_CC_Recipient { get; set; }
        public string CS_Key1Name { get; set; }
        public string CS_Key2Name { get; set; }
        public string CS_Key3Name { get; set; }
        public string CS_Key4Name { get; set; }
        public string CS_Key5Name { get; set; }

        public eHubClient CS_CC_RecipientNavigation { get; set; }
        public eHubClient CS_CC_SenderNavigation { get; set; }
        public eHubTransformationSet CS_TSNavigation { get; set; }
        public ICollection<eHubCodeMapKey> eHubCodeMapKey { get; set; }
        public ICollection<eHubCodeSetResult> eHubCodeSetResult { get; set; }
    }
}
