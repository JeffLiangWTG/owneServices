using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.SalesTeam)]
	public class SalesTeamCollection : ActiveBusinessObjectCollection<SalesTeam>
	{
		public SalesTeamCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public SalesTeamCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public SalesTeamCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public SalesTeamCollection(GlbStaff staff)
			: base(staff, typeof(GlbGroupLink), new ZQuery(GlbGroupSchema.GG_IsSales, true), GlbGroupLinkSchema.GK_GS, GlbGroupLinkSchema.GK_GG)
		{
		}

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(GlbGroupSchema.GG_IsSales, true);
			return query;
		}

		#endregion
	}
}
