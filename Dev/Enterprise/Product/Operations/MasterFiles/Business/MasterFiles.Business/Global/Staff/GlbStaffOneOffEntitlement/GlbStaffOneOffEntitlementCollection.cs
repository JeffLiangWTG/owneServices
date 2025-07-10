using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffOneOffEntitlementCollection : ActiveBusinessObjectCollection<GlbStaffOneOffEntitlement>
	{
		public GlbStaffOneOffEntitlementCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffOneOffEntitlementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffOneOffEntitlementCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
