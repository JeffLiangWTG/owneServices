using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsSerialNumberPivotSelectorCollection : NonPersistentBusinessObjectCollection<WhsSerialNumberPivotSelector>
	{
		public event EventHandler SelectionChanged;
		readonly Dictionary<ZGuid, List<WhsSerialNumberPivotSelector>> SerialByInventoryPK;

		public WhsSerialNumberPivotSelectorCollection(IEnumerable<WhsSerialNumberPivot> serialNumberPivots, WhsPick pick, ZString serialNumberSpecified, Func<ZGuid, bool> readOnlyForChangingAllocationsProvider)
			: base()
		{
			Argument.NotNull(serialNumberPivots, nameof(serialNumberPivots));
			Argument.NotNull(pick, nameof(pick));
			Argument.NotNull(readOnlyForChangingAllocationsProvider, nameof(readOnlyForChangingAllocationsProvider));

			var allPickLines = new Lazy<HashSet<ZGuid>>(
				pick.GetAllPickLines()
					.Select(pl => pl.PK)
					.ToHashSet
			);

			var serialNumberSelectors = serialNumberPivots
				.Where(p => MatchSpecifiedSerialNumber(p) && MatchPickingLine(allPickLines, p))
				.OrderBy(p => p.SerialNumberValue)
				.Select(CreateSelector)
				.ToArray();

			SerialByInventoryPK = BuildSerialByInventoryMap(serialNumberSelectors);

			bool MatchSpecifiedSerialNumber(WhsSerialNumberPivot pivot) => serialNumberSpecified.IsEmpty || pivot.SerialNumberValue.EqualsIgnoringCase(serialNumberSpecified);

			WhsSerialNumberPivotSelector CreateSelector(WhsSerialNumberPivot pivot)
			{
				var whsSerialNumberPivotSelector = new WhsSerialNumberPivotSelector(pivot, readOnlyForChangingAllocationsProvider);
				whsSerialNumberPivotSelector.OnSelectionChanged += (s, e) => SelectionChanged?.Invoke(s, e);
				return whsSerialNumberPivotSelector;
			}
		}

		public WhsSerialNumberPivotSelectorCollection(ILineWithCommittedPickLines lineWithCommittedPickLines, bool allocateDefaultSerial)
			: base()
		{
			Argument.NotNull(lineWithCommittedPickLines, nameof(lineWithCommittedPickLines));

			var allocated = 0;
			var pickLines = lineWithCommittedPickLines.PickLines;
			var pickLinePKs = new Lazy<HashSet<ZGuid>>(
				pickLines
					.Select(pl => pl.PK)
					.ToHashSet
			);

			var serialNumberPivots = pickLines
			  .Select(p => new { PickLine = p, p.InventoryLine })
			  .SelectMany(o => ((ISerialNumberParent)o.InventoryLine).SerialNumbers.Select(s => new { Pivot = s, o.PickLine }));

			var serialNumberSelectors = serialNumberPivots
				.Where(p => MatchPickingLine(pickLinePKs, p.Pivot))
				.OrderBy(p => p.Pivot.SerialNumberValue)
				.Select(p => CreateSelectorWithEvent(p.Pivot, p.PickLine))
				.ToArray();

			SerialByInventoryPK = BuildSerialByInventoryMap(serialNumberSelectors);

			WhsSerialNumberPivotSelector CreateSelectorWithEvent(WhsSerialNumberPivot pivot , WhsPickLine pickLine)
			{
				var selector = new WhsSerialNumberPivotSelector(pivot, pickLine);

				if (allocateDefaultSerial &&
					pivot.WSV_WZ_PickingLine.IsEmpty &&
					allocated < lineWithCommittedPickLines.TransactionQty)
				{
					pivot.WSV_WZ_PickingLine = pickLine.PK;
					allocated++;
				}

				selector.OnSelectionChanged += (s, e) => SelectionChanged?.Invoke(s, e);
				return selector;
			}
		}

		WhsSerialNumberPivotSelectorCollection()
			: base()
		{
		}

		static bool MatchPickingLine(Lazy<HashSet<ZGuid>> picklines, WhsSerialNumberPivot pivot) => pivot.WSV_WZ_PickingLine.IsEmpty || picklines.Value.Contains(pivot.WSV_WZ_PickingLine);

		Dictionary<ZGuid, List<WhsSerialNumberPivotSelector>> BuildSerialByInventoryMap(WhsSerialNumberPivotSelector[] serialNumberSelectors)
		{
			AddRange(serialNumberSelectors);

			return serialNumberSelectors
				.GroupBy(s => s.InventoryPK)
				.ToDictionary(k => k.Key, v => v.ToList());
		}

		public static WhsSerialNumberPivotSelectorCollection Empty => new WhsSerialNumberPivotSelectorCollection();

		public IEnumerable<WhsSerialNumberPivotSelector> GetSerialsByInventory(ZGuid inventoryPK) => SerialByInventoryPK[inventoryPK];

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() =>
			throw new InvalidOperationException("Creating new items directly is not supported for this collection.");
	}
}
