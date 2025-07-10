using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class TaskManagementContextFact : ITaskManagementContextFact
	{
		public TaskManagementContextFact(int maxNumberOfLinesFallBack)
		{
			MaxNumberOfLinesFallBack = maxNumberOfLinesFallBack;
		}

		public int MaxNumberOfLinesFallBack { get; }
	}
}
