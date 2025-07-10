using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEntitlementTypeCollection : ActiveBusinessObjectCollection<GlbStaffEntitlementType>
	{
		public GlbStaffEntitlementTypeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffEntitlementTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffEntitlementTypeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
