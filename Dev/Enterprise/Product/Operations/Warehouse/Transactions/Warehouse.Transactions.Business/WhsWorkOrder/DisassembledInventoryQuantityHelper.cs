using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class DisassembledInventoryQuantityHelper : WorkOrderDisassemblyHelper
	{
		public DisassembledInventoryQuantityHelper(WhsWorkOrder workOrder)
			: base(workOrder)
		{
			var relatedPickLines = workOrder.DisassemblyLinesForPick.SelectMany(k => k.PickLines).Select(pl => pl.InventoryLine.WE_WE_OriginalDocketLineForRating).Distinct();
			workOrder.Factory.AddFetchHint(typeof(WhsBOMInventoryPivot), new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, relatedPickLines));
		}

		public decimal GetDisassembledQuantity() => ProcessDisassembly();

		protected override decimal ProcessLinkedInventory(WhsComponentOrderLine componentLine, WhsComponentOrderLine kitLine, WhsPickLine referencePickLine, decimal quantity)
			=> quantity;

		protected override decimal ProcessDisassemblyWithoutLinkedInventory(WhsWorkOrderLine componentLine, decimal quantityWithoutLinks)
			=> WhsWorkOrderHelper.GetDisassemblyQuantityFromLine(componentLine, quantityWithoutLinks);
	}
}
