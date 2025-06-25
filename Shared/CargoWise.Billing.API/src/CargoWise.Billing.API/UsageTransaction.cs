using System;

namespace CargoWise.Billing.API
{
	[Serializable]
	public sealed class UsageTransaction : ELKTransaction
	{
		public int UsageCount { get; set; }
		public string EnterpriseCode { get; set; }
		public string ServerCode { get; set; }
		public string Environment { get; set; }
		public string CompanyCode { get; set; }
		public string CompanyName { get; set; }
		public string BranchCode { get; set; }
		public string UsageCode { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj is UsageTransaction other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = UsageCount;
				hashCode = (hashCode * 397) ^ ServiceOccuredUTC.GetHashCode();
				hashCode = (hashCode * 397) ^ (AdditionalRefs != null ? AdditionalRefs.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (EnterpriseCode != null ? EnterpriseCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ServerCode != null ? ServerCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Environment != null ? Environment.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CompanyCode != null ? CompanyCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CompanyName != null ? CompanyName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (BranchCode != null ? BranchCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (UsageCode != null ? UsageCode.GetHashCode() : 0);
				return hashCode;
			}
		}

		bool Equals(UsageTransaction other)
		{
			return
				UsageCount == other.UsageCount &&
				ServiceOccuredUTC.Equals(other.ServiceOccuredUTC) &&
				string.Equals(AdditionalRefs, other.AdditionalRefs) &&
				string.Equals(EnterpriseCode, other.EnterpriseCode) &&
				string.Equals(ServerCode, other.ServerCode) &&
				string.Equals(Environment, other.Environment) &&
				string.Equals(CompanyCode, other.CompanyCode) &&
				string.Equals(CompanyName, other.CompanyName) &&
				string.Equals(BranchCode, other.BranchCode) &&
				string.Equals(UsageCode, other.UsageCode);
		}

		public override string ToString()
		{
			return string.Format("Count: {0}, ServiceOccuredUTC: {1}, AdditionalRefs: {2}, EnterpriseCode: {3}, ServerCode: {4}, Environment: {5}, CompanyCode: {6}, CompanyName: {7}, BranchCode: {8}, UsageCode: {9}",
				UsageCount,
				ServiceOccuredUTC.ToString("s"),
				AdditionalRefs,
				EnterpriseCode,
				ServerCode,
				Environment,
				CompanyCode,
				CompanyName,
				BranchCode,
				UsageCode);
		}

	}
}
