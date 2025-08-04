using System.Runtime.Serialization;

namespace CargoWise.Billing.Service
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/CargoWise.eServices.Billing.WcfService")]
    public sealed class LicenseInfo
    {
        [DataMember]
        public string EnterpriseCode { get; set; }

        [DataMember]
        public int DatabaseNumber { get; set; }

        [DataMember]
        public string ServerCode { get; set; }

        [DataMember]
        public string HostedLocation { get; set; }

        [DataMember]
        public string LicenseType { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public bool IsTeardownInProgress { get; set; }
    }
}
