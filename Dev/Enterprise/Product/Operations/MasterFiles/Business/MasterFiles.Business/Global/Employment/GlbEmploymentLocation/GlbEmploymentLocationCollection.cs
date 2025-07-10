using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmploymentLocationCollection : ActiveBusinessObjectCollection<GlbEmploymentLocation>
	{
		public GlbEmploymentLocationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbEmploymentLocationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbEmploymentLocationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
