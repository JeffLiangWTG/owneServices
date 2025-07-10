using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffRemunerationCollection : ActiveBusinessObjectCollection<GlbStaffRemuneration>
	{
		public GlbStaffRemunerationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffRemunerationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffRemunerationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
