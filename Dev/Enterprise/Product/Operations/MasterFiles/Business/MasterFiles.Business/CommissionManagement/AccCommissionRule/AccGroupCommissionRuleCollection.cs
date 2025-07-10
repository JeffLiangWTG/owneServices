using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGroupCommissionRuleCollection : ActiveBusinessObjectCollection<AccGroupCommissionRule>
	{
		public AccGroupCommissionRuleCollection(SalesTeam salesTeam)
			: base(salesTeam)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(AccCommissionRuleSchema.ACM_GS_NKStaff, ZString.Empty);
			return query;
		}
	}
}
