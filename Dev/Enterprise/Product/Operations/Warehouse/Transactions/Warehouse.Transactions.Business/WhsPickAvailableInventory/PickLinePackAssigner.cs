using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class PickLinePackAssigner
	{
		#region AssignPackTypes

		public static IReadOnlyList<WhsPickLine> AssignPackTypes(IReadOnlyList<WhsPickLine> pickLines, ForceWhsPickUOMTypeAllocation forcePickUOMTypeAllocation)
		{
			var allPickLines = new List<WhsPickLine>(pickLines);
			if (pickLines.Count > 0)
			{
				var product = pickLines[0].SupplierPart;
				if (pickLines.Any(x => x.SupplierPart != product))
				{
					throw new InvalidOperationException("all pickLines must be related to same product when attempting to assign pack types.");
				}

				var qtyPicked = 0m;

				foreach (var pickLine in pickLines)
				{
					pickLine.WZ_F3_NKAllocatedPackType = "";
					qtyPicked += pickLine.WZ_Units;
				}
				var conversionsTable = GetConversionsToSKUTable(forcePickUOMTypeAllocation, product);

				var groups = BiggestPackTypeGrouper.GetGroups(product, qtyPicked, conversionsTable);
				// assigning pack types starting from the biggest

				var pickLinesWithoutPackTypes = new List<WhsPickLine>(pickLines.Where(pl => pl.WZ_F3_NKAllocatedPackType.IsEmpty));
				foreach (var group in groups.OrderByDescending(g => g.Qty))
				{
					allPickLines.AddRange(AssignPackTypeToPickLines(pickLinesWithoutPackTypes, group));
				}
			}

			return allPickLines;
		}

		static ConversionsToSKUTable GetConversionsToSKUTable(ForceWhsPickUOMTypeAllocation forcePickUOMTypeAllocation, OrgSupplierPart product)
		{
			ConversionsToSKUTable result = null;
			switch (forcePickUOMTypeAllocation)
			{
				case ForceWhsPickUOMTypeAllocation.Case:
					result = new ConversionsToSKUTableForceCASPackType(product);
					break;
				case ForceWhsPickUOMTypeAllocation.SplitCase:
					result = new ConversionsToSKUTableForceSPCPackType(product);
					break;
			}

			return result;
		}

		#region AssignPackTypeToPickLines

		static IEnumerable<WhsPickLine> AssignPackTypeToPickLines(List<WhsPickLine> pickLines, GroupedQuantityItem group)
		{
			var splitPickLines = new List<WhsPickLine>(pickLines.Count);

			var qtyToAssign = group.Qty;
			while (qtyToAssign > 0)
			{
				var pickLineWithIndex = pickLines
					.Select((pl, index) => new { Index = index, PickLine = pl })
					.OrderBy(pl => pl.PickLine.WZ_Units == qtyToAssign ? 0 : 1) // find match first 
					.ThenByDescending(pl => pl.PickLine.WZ_Units).First(); // process biggest picklines first

				var pickLine = pickLineWithIndex.PickLine;
				if (pickLine.WZ_Units > qtyToAssign)
				{
					//have to split
					pickLine = pickLine.Split(qtyToAssign);
					splitPickLines.Add(pickLine);
				}
				else
				{
					pickLines.RemoveAt(pickLineWithIndex.Index);
				}

				pickLine.WZ_F3_NKAllocatedPackType = group.PackType;
				qtyToAssign -= pickLine.WZ_Units;
			}

			return splitPickLines;
		}

		#endregion

		#endregion
	}
}
