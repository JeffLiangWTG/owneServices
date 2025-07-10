using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffClassificationCollection : ActiveBusinessObjectCollection<GlbStaffClassification>
	{
		public GlbStaffClassificationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffClassificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffClassificationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
