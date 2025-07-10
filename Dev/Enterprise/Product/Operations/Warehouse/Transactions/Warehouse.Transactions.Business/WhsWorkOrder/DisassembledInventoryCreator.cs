using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	class DisassembledInventoryCreator : WorkOrderDisassemblyHelper
	{
		public DisassembledInventoryCreator(WhsComponentOrder workOrder, WhsReceive receive, ZDateTimeOffset startTime)
			: base(workOrder)
		{
			Pick = Argument.NotNull(workOrder.Pick, nameof(workOrder.Pick));
			Receive = Argument.NotNull(receive, nameof(receive));
			StagingLocationHelper = new WorkOrderStagingLocationHelper(workOrder.Warehouse);
			StartTime = startTime;
		}

		readonly WhsPick Pick;
		readonly WhsReceive Receive;
		readonly WorkOrderStagingLocationHelper StagingLocationHelper;
		readonly ZDateTimeOffset StartTime;

		public void CreateDisassembledInventory() => ProcessDisassembly();

		protected override decimal ProcessLinkedInventory(WhsComponentOrderLine componentLine, WhsComponentOrderLine kitLine, WhsPickLine referencePickLine, decimal quantity)
		{
			var receiveLine = WhsWorkOrderHelper.AddNewInventoryFromWorkOrderLine(StagingLocationHelper, Pick, Receive, componentLine, kitLine, quantity, StartTime);
			WhsWorkOrderHelper.CopyAttributesFromPickedInventory(receiveLine, referencePickLine);

			return 0m;
		}

		protected override decimal ProcessDisassemblyWithoutLinkedInventory(WhsWorkOrderLine componentLine, decimal quantityWithoutLinks)
		{
			WhsWorkOrderHelper.AddNewInventoryFromWorkOrderLine(StagingLocationHelper, Pick, Receive, componentLine, null, WhsWorkOrderHelper.GetDisassemblyQuantityFromLine(componentLine, quantityWithoutLinks), StartTime);

			return 0m;
		}
	}
}
