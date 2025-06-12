using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubCertificate
    {
        public Guid CE_PK { get; set; }
        public string CE_Category { get; set; }
        public Guid? CE_EH_Owner { get; set; }
        public string CE_ID { get; set; }
        public Guid? CE_CC_Owner { get; set; }
        public DateTime CE_AddedUTC { get; set; }
        public string CE_ContainerType { get; set; }
        public byte[] CE_BinaryContainer { get; set; }
        public string CE_TextContainer { get; set; }
        public string CE_Password { get; set; }
        public DateTime? CE_ValidFromUTC { get; set; }
        public DateTime? CE_ValidToUTC { get; set; }
        public DateTime? CE_ActiveFromUTC { get; set; }
        public string CE_Thumbprint { get; set; }
        public string CE_Issuer { get; set; }
        public string CE_SerialNumber { get; set; }
        public string CE_SubjectKeyIdentifier { get; set; }

        public eHubClient CE_CC_OwnerNavigation { get; set; }
        public eHubClientSystem CE_EH_OwnerNavigation { get; set; }
    }
}
