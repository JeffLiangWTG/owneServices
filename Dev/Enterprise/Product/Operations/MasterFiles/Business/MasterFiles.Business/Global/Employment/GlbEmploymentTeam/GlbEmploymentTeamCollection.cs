using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmploymentTeamCollection : ActiveBusinessObjectCollection<GlbEmploymentTeam>
	{
		public GlbEmploymentTeamCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbEmploymentTeamCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbEmploymentTeamCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
