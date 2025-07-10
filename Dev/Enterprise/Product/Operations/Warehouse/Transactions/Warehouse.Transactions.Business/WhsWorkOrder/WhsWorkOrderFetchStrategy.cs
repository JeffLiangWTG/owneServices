using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderFetchStrategy : WhsPickableDocketFetchStrategy
	{
		public WhsWorkOrderFetchStrategy(WhsWorkOrder docket)
			: base(docket)
		{
		}

		protected WhsWorkOrder WorkOrder => (WhsWorkOrder)BusinessObject;

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();

			// tested in TestTotalUnitsValidation_Assembly_IPRSecondaryProducts_DBHits
			if (WorkOrder.WD_IsInwardsProcessingJob)
			{
				foreach (var docketLine in WorkOrder.AssemblyLinesForReceive)
				{
					Factory.AddFetchHint(OrgSecondaryPartBOMSchema.OSB_OP_MainProduct, docketLine.WE_OP);
				}

				foreach (var docketLine in WorkOrder.AllLines)
				{
					Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, docketLine.PK);
				}
			}
		}
	}
}
