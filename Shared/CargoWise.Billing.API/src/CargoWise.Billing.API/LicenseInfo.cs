using System;

namespace CargoWise.Billing.API
{
    [Serializable]
    public sealed class LicenseInfo
    {
        public string EnterpriseCode { get; set; }
        public int DatabaseNumber { get; set; }
        public string ServerCode { get; set; }
        public string HostedLocation { get; set; }
        public string LicenseType { get; set; }
        public bool IsActive { get; set; }
        public bool IsTeardownInProgress { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is LicenseInfo other &&
                   string.Equals(EnterpriseCode, other.EnterpriseCode) &&
                   DatabaseNumber == other.DatabaseNumber &&
                   string.Equals(ServerCode, other.ServerCode) &&
                   string.Equals(HostedLocation, other.HostedLocation) &&
                   string.Equals(LicenseType, other.LicenseType) &&
                   IsActive == other.IsActive &&
                   IsTeardownInProgress == other.IsTeardownInProgress;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = EnterpriseCode != null ? EnterpriseCode.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ DatabaseNumber;
                hashCode = (hashCode * 397) ^ (ServerCode != null ? ServerCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (HostedLocation != null ? HostedLocation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LicenseType != null ? LicenseType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsActive.GetHashCode();
                hashCode = (hashCode * 397) ^ IsTeardownInProgress.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"EnterpriseCode: {EnterpriseCode}, DatabaseNumber: {DatabaseNumber}, ServerCode: {ServerCode}, HostedLocation: {HostedLocation}, LicenseType: {LicenseType}, IsActive: {IsActive}, IsTeardownInProgress: {IsTeardownInProgress}";
        }
    }
}
