using System;
using CargoWise.Common;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class CycleCountLocationCoreFact : LocationFact, ICycleCountLocationCoreFact, ITaskManagementGroupingFact
	{
		public CycleCountLocationCoreFact(
			Guid pk,
			Guid entityPK,
			string locationTypeCode,
			string locationClass,
			string areaName,
			string rowName,
			int column,
			int level,
			int tray,
			string locationString,
			string locationStatus,
			string pickMethod,
			bool cycleCountTaskExists,
			int cycleCountPathSequence,
			int rowPathSequence,
			int locationStringSortIndex,
			DateTime? inventoryLastChangedDate,
			DateTime? cycleCountLastPerformedDate,
			decimal stockOnHand,
			bool hasCommittedStock,
			string granularity,
			int priority)
			: base(pk, locationTypeCode, locationClass, areaName, rowName, column, level, tray, locationStatus, false, false)
		{
			EntityPK = entityPK;
			CycleCountTaskExists = cycleCountTaskExists;
			CycleCountPathSequence = cycleCountPathSequence;
			RowPathSequence = rowPathSequence;
			PickMethod = Argument.NotNull(pickMethod, nameof(pickMethod));
			LocationString = locationString;
			LocationStringSortIndex = locationStringSortIndex;
			InventoryLastChangedDate = inventoryLastChangedDate;
			CycleCountLastPerformedDate = cycleCountLastPerformedDate;
			StockOnHand = stockOnHand;
			HasCommittedStock = hasCommittedStock;
			Granularity = Argument.NotNull(granularity, nameof(granularity));
			Priority = cycleCountTaskExists && priority == 0 ? 21 : priority;
		}

		public Guid EntityPK { get; }

		public bool CycleCountTaskExists { get; set; }

		public int CycleCountPathSequence { get; }

		public int RowPathSequence { get; }

		public string PickMethod { get; }

		public string LocationString { get; }

		public int LocationStringSortIndex { get; }

		public DateTime? InventoryLastChangedDate { get; }

		public DateTime? CycleCountLastPerformedDate { get; }

		public decimal StockOnHand { get; set; }

		public bool HasCommittedStock { get; set; }

		public int Priority { get; }

		public string Granularity { get; }

		int ITaskManagementGroupingFact.NumberOfLines => 1;
		int ITaskManagementGroupingFact.NumberOfUnits => 0;
		int ITaskManagementGroupingFact.NumberOfPacks => 0;
		decimal ITaskManagementGroupingFact.Weight => 0;
		string ITaskManagementGroupingFact.WeightUQ => string.Empty;
		decimal ITaskManagementGroupingFact.Volume => 0;
		string ITaskManagementGroupingFact.VolumeUQ => string.Empty;
		Guid ITaskManagemementTaskLink.PK => EntityPK;
		Guid ITaskManagemementTaskLink.AssignedTask { get; set; }
	}
}
