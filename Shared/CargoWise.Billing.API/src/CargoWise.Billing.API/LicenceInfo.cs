using System;

namespace CargoWise.Billing.API
{
    [Serializable]
    public sealed class LicenceInfo
    {
        public string EnterpriseCode { get; set; }
        public int DatabaseNumber { get; set; }
        public string ServerCode { get; set; }
		public string HostedLocation { get; set; }
		public string Product { get; set; }
		public bool IsActive { get; set; }
        public bool IsTeardownInProgress { get; set; }
        public string LicenceType { get; set; }
    }
}
