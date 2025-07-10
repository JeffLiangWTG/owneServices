using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class TaskManagementCycleCountLocationFact : CycleCountLocationFact, ITaskManagementCycleCountLocationFact
	{
		public TaskManagementCycleCountLocationFact(
			CycleCountLocationCoreFact location,
			IOrganisationFact client,
			ICycleCountProductFact product,
			decimal stockOnHandForThisProduct)
			: base(location, client, product, stockOnHandForThisProduct)
		{
			Grouping = new FactJoin<ITaskManagementGroupingFact>(Argument.NotNull(location, nameof(location)));
			Priority = location.Priority;
			Granularity = location.Granularity;
		}

		public FactJoin<ITaskManagementGroupingFact> Grouping { get; }

		public int Priority { get; }

		public string Granularity { get; }
	}
}
