using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public class UnmatchOrgRecordComparer : IEqualityComparer<UnmatchOrgRecord>
	{
		public bool Equals(UnmatchOrgRecord x, UnmatchOrgRecord y)
		{
			return x.OrganisationType == y.OrganisationType
						 && x.OrganisationSubType == y.OrganisationSubType
						 && x.DocAddressType == y.DocAddressType;
		}

		public int GetHashCode(UnmatchOrgRecord obj)
		{
			return obj.OrganisationType.GetHashCode()
						 ^ obj.OrganisationSubType.GetHashCode()
						 ^ obj.DocAddressType.GetHashCode();
		}
	}
}
