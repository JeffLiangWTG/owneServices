using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEntitlementCollection : ActiveBusinessObjectCollection<GlbStaffEntitlement>
	{
		public GlbStaffEntitlementCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffEntitlementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffEntitlementCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
