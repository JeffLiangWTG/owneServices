using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Rating
{
	class OrgWithSourceComparer : IEqualityComparer<OrgWithSource>
	{
		public bool Equals(OrgWithSource x, OrgWithSource y)
		{
			return BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer.Equals(x, y);
		}

		public int GetHashCode(OrgWithSource obj)
		{
			return obj == null ? 0 : BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer.GetHashCode(obj.Org);
		}
	}
}
