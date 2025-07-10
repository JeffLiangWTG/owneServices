using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateCommodityDefaultingRuleCollection : ActiveBusinessObjectCollection<OrgRateCommodityDefaultingRule>
	{
		public OrgRateCommodityDefaultingRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgRateCommodityDefaultingRuleCollection(BusinessObjectFactory factory, OrgHeader master)
			: base(factory, master)
		{
			orgHeader = master;
		}

		public OrgRateCommodityDefaultingRuleCollection(OrgHeader master)
			: base(master)
		{
			orgHeader = master;
		}

		#region Master

		readonly OrgHeader orgHeader;

		#endregion

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = new ZQuery();

			if (orgHeader != null)
			{
				filter.AddToFilter(JoinCondition.Or, OrgRateCommodityDefaultingRuleSchema.ORC_OH, SQLComparisonOperator.Equal, orgHeader.PK);
			}
			return filter;
		}
	}
}
