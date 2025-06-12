using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubClientSystem
    {
        public eHubClientSystem()
        {
            eHubCertificate = new HashSet<eHubCertificate>();
            eHubClientSystemRegistration = new HashSet<eHubClientSystemRegistration>();
            eHubITCustomsJobStatus = new HashSet<eHubITCustomsJobStatus>();
        }

        public Guid EH_PK { get; set; }
        public string EH_ID { get; set; }
        public string EH_URL { get; set; }
        public DateTime EH_InsertUTC { get; set; }
        public DateTime EH_LastUpdateUTC { get; set; }

        public ICollection<eHubCertificate> eHubCertificate { get; set; }
        public ICollection<eHubClientSystemRegistration> eHubClientSystemRegistration { get; set; }
        public ICollection<eHubITCustomsJobStatus> eHubITCustomsJobStatus { get; set; }
    }
}
