using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbTeamCollection : ActiveBusinessObjectCollection<GlbTeam>
	{
		public GlbTeamCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

