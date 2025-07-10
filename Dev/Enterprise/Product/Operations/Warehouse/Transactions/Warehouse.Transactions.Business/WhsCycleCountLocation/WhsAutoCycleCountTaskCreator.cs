using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAutoCycleCountTaskCreator : IWhsAutoCycleCountTaskCreator
	{
		public WhsAutoCycleCountTaskCreator(IWhsCycleCountLocationCreator cycleCountLocationCreator)
		{
			CycleCountLocationCreator = Argument.NotNull(cycleCountLocationCreator, nameof(cycleCountLocationCreator));
		}

		IWhsCycleCountLocationCreator CycleCountLocationCreator { get; }

		public void BeforeAutoTasksCreation(IReadOnlyCollection<WhsPickAvailableInventory> availableInventories, BusinessObjectFactory factory)
		{
		}

		public void CreateInventoryAccuracyManagementTasksForAutoTouchCount(IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>> groupedAvailableInventoryLines, WhsWarehouse warehouse)
		{
			CreateAutomatedCycleCounts(groupedAvailableInventoryLines);
		}

		public void CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>> groupedAvailableInventoryLines, WhsWarehouse warehouse)
		{
			CreateAutomatedCycleCounts(groupedAvailableInventoryLines, priority: 1);
		}

		void CreateAutomatedCycleCounts(IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>> groupedAvailableInventoryLines, int priority = 0)
		{
			foreach (var location in groupedAvailableInventoryLines.Select(l => l.Key).Where(l => !LocationsWithCycleCount.Contains(l.PK)))
			{
				var cycleCount = CycleCountLocationCreator.CreateCycleCountLocation(location, priority);
				if (cycleCount != null)
				{
					CycleCountsForRollingBack.Add(cycleCount);
				}
				LocationsWithCycleCount.Add(location.PK);
			}
		}

		public void ClearAutoCreatedTasks(bool requireRollback)
		{
			if (requireRollback)
			{
				foreach (var cycleCount in CycleCountsForRollingBack)
				{
					cycleCount.Delete();
				}
			}

			LocationsWithCycleCount.Clear();
			CycleCountsForRollingBack.Clear();
		}

		List<WhsCycleCountLocation> CycleCountsForRollingBack => cycleCountsForRollingBack ?? (cycleCountsForRollingBack = new List<WhsCycleCountLocation>());
		List<WhsCycleCountLocation> cycleCountsForRollingBack;

		HashSet<ZGuid> LocationsWithCycleCount => locationsWithCycleCount ?? (locationsWithCycleCount = new HashSet<ZGuid>());
		HashSet<ZGuid> locationsWithCycleCount;
	}
}
