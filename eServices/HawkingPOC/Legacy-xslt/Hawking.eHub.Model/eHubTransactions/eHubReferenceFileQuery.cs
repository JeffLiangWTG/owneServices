using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubReferenceFileQuery
    {
        public eHubReferenceFileQuery()
        {
            eHubReferenceFileCache = new HashSet<eHubReferenceFileCache>();
        }

        public Guid RF_PK { get; set; }
        public string RF_ReferenceID { get; set; }
        public string RF_Description { get; set; }
        public string RF_ApplicationCode { get; set; }
        public string RF_Type { get; set; }
        public string RF_Query { get; set; }
        public Guid? RF_CC_Provider { get; set; }
        public Guid? RF_CC_Requestor { get; set; }
        public short? RF_RetentionCount { get; set; }

        public eHubClient RF_CC_ProviderNavigation { get; set; }
        public eHubClient RF_CC_RequestorNavigation { get; set; }
        public ICollection<eHubReferenceFileCache> eHubReferenceFileCache { get; set; }
    }
}
