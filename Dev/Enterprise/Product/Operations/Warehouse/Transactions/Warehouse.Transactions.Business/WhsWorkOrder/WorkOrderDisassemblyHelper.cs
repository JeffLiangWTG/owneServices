using System;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	abstract class WorkOrderDisassemblyHelper
	{
		protected WorkOrderDisassemblyHelper(WhsComponentOrder workOrder)
		{
			WorkOrder = Argument.NotNull(workOrder, nameof(workOrder));
		}

		readonly WhsComponentOrder WorkOrder;

		protected decimal ProcessDisassembly()
		{
			var result = 0m;

			var disassemblyLines = Lazy.Create(() => WorkOrder.DisassemblyLinesForPutaway.Select(l => l.PK).ToHashSet());
			// disassemble one level down, user can create more DIS WO's to go further
			foreach (var kit in WorkOrder.DisassemblyLinesForPick.Cast<WhsComponentOrderLine>())
			{
				var quantityWithoutLinks = 0m;
				var withoutLinks = kit.PickLines.Count == 0;
				var bom = kit.SupplierPart.BillOfMaterials;

				foreach (var kitPickLine in kit.PickLines)
				{
					var hadNoLinks = true;

					foreach (var link in kitPickLine.InventoryLine.BOMComponentLinks)
					{
						hadNoLinks = false;

						// We have to copy Attributes/CustomsData, so we go through the PickLines of the ComponentLines to find the inventory
						var referenceLine = (WhsComponentOrderLine)link.ComponentLine;
						var parentLine = referenceLine.ParentLine;

						var orgPartBOM = bom.FindByComponentPKandPackType(referenceLine.WE_OP, referenceLine.WE_F3_NKPackType);
						if (orgPartBOM?.OE_CanReuse ?? true)
						{
							// In case of any changes made to the BOM Definition, we will get the quantity from the original line
							var quantityInOneAssembly = referenceLine.WE_TransactionQuantity / parentLine.WE_TransactionQuantity;
							var quantityToDisassemble = kitPickLine.WZ_Units * quantityInOneAssembly;

							foreach (var componentPickLineGroup in referenceLine.PickLines.GroupBy(pl => pl.InventoryLinePKForAvailableInventory))
							{
								var units = componentPickLineGroup.Sum(pl => pl.WZ_Units);
								var unitsToDisassemble = Math.Min(quantityToDisassemble, units);
								result += ProcessLinkedInventory(referenceLine, kit, componentPickLineGroup.First(), unitsToDisassemble);

								quantityToDisassemble -= unitsToDisassemble;
								if (quantityToDisassemble == 0m)
								{
									break;
								}
							}
						}
					}

					if (hadNoLinks)
					{
						withoutLinks = true;
						quantityWithoutLinks += kitPickLine.WZ_Units;
					}
				}

				if (withoutLinks)
				{
					if (WorkOrder is WhsDynamicWorkOrder)
					{
						throw new InvalidOperationException("Attempted to Revert Assembly with Dynamic Work Order for inventory with no Child Component Links.");
					}
					else
					{
						foreach (var componentLine in kit.ChildComponentLines.Cast<WhsWorkOrderLine>().Where(l => disassemblyLines.Value.Contains(l.PK)))
						{
							result += ProcessDisassemblyWithoutLinkedInventory(componentLine, quantityWithoutLinks);
						}
					}
				}
			}

			return result;
		}

		protected abstract decimal ProcessLinkedInventory(WhsComponentOrderLine referenceLine, WhsComponentOrderLine kitLine, WhsPickLine referencePickLine, decimal quantity);

		protected abstract decimal ProcessDisassemblyWithoutLinkedInventory(WhsWorkOrderLine componentLine, decimal quantityWithoutLinks);
	}
}
