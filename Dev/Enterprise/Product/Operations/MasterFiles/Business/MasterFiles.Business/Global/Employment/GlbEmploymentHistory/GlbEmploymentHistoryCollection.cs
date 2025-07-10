using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmploymentHistoryCollection : ActiveBusinessObjectCollection<GlbEmploymentHistory>
	{
		public GlbEmploymentHistoryCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbEmploymentHistoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbEmploymentHistoryCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
