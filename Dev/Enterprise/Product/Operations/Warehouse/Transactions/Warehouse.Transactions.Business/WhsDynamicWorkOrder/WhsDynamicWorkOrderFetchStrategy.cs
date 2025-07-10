using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderFetchStrategy : WhsPickableDocketFetchStrategy
	{
		public WhsDynamicWorkOrderFetchStrategy(WhsDynamicWorkOrder docket)
			: base(docket)
		{
		}

		protected WhsDynamicWorkOrder WorkOrder => (WhsDynamicWorkOrder)BusinessObject;

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();

			// tested in TestTotalUnitsValidation_Assembly_IPRSecondaryProducts_DBHits
			if (WorkOrder.WD_IsInwardsProcessingJob)
			{
				foreach (var docketLine in WorkOrder.AllLines)
				{
					Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, docketLine.PK);
				}
			}
		}
	}
}
