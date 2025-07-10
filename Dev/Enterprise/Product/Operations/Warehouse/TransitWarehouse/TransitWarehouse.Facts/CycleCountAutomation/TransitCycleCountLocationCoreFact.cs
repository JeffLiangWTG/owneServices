using System;
using Enterprise.Warehouse.Transactions.Facts;
using WTG.ProductionRules.Business.TransitWarehouseCycleCountAutomation;

namespace Enterprise.Warehouse.Transit.Facts
{
	public class TransitCycleCountLocationCoreFact : LocationFact, ITransitCycleCountLocationCoreFact
	{
		public TransitCycleCountLocationCoreFact(
			Guid pk,
			string locationTypeCode,
			string locationClass,
			string areaName,
			string rowName,
			int column,
			int level,
			int tray,
			string locationString,
			string locationStatus,
			bool cycleCountTaskExists,
			int cycleCountPathSequence,
			int rowPathSequence,
			int locationStringSortIndex,
			DateTime? inventoryLastChangedDate,
			DateTime? cycleCountLastPerformedDate)
			: base(pk, locationTypeCode, locationClass, areaName, rowName, column, level, tray, locationStatus, false, true)
		{
			CycleCountTaskExists = cycleCountTaskExists;
			CycleCountPathSequence = cycleCountPathSequence;
			RowPathSequence = rowPathSequence;
			LocationString = locationString;
			LocationStringSortIndex = locationStringSortIndex;
			InventoryLastChangedDate = inventoryLastChangedDate;
			CycleCountLastPerformedDate = cycleCountLastPerformedDate;
			InventoryChanged = inventoryLastChangedDate.HasValue && (!cycleCountLastPerformedDate.HasValue || inventoryLastChangedDate.Value > cycleCountLastPerformedDate.Value);
		}

		public bool CycleCountTaskExists { get; set; }

		public int CycleCountPathSequence { get; }

		public int RowPathSequence { get; }

		public string LocationString { get; }

		public int LocationStringSortIndex { get; }

		public DateTime? CycleCountLastPerformedDate { get; }

		public DateTime? InventoryLastChangedDate { get; }

		public bool InventoryChanged { get; }
	}
}
