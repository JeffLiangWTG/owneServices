using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountLocationVarianceCollection : ActiveBusinessObjectCollection<WhsCycleCountLocationVariance>
	{
		public WhsCycleCountLocationVarianceCollection(WhsCycleCountLocation master)
			: base(master.Factory, master, null, WhsCycleCountLocationVarianceSchema.WCC_WCL_CycleCountLocation)
		{
		}

		public WhsCycleCountLocation CycleCount
		{
			get
			{
				var matchingParentLine = MatchingParentLine;
				return matchingParentLine == null
					? (WhsCycleCountLocation)Relationship.Master
					: matchingParentLine.CycleCount;
			}
		}

		WhsCycleCountLocationVariance MatchingParentLine
		{
			get { return Relationship.Master as WhsCycleCountLocationVariance; }
		}
	}
}
