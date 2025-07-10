using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAutoInventoryAccuracyManager : IWhsAutoInventoryAccuracyManager
	{
		public WhsAutoInventoryAccuracyManager(IWhsAutoCycleCountTaskCreator autoCycleCountTaskCreator, IWhsAutoStocktakeTaskCreator autoStocktakeTaskCreator)
		{
			AutoCycleCountTaskCreator = Argument.NotNull(autoCycleCountTaskCreator, nameof(autoCycleCountTaskCreator));
			AutoStocktakeTaskCreator = Argument.NotNull(autoStocktakeTaskCreator, nameof(autoStocktakeTaskCreator));

			TaskCreator = new Lazy<IWhsAutoInventoryAccuracyManagementTaskCreator>(() =>
			{
				return WarehouseDataRegistry.Instance.IsUsingLegacyStocktake
					? AutoStocktakeTaskCreator
					: AutoCycleCountTaskCreator;
			});
		}

		IWhsAutoCycleCountTaskCreator AutoCycleCountTaskCreator { get; }
		IWhsAutoStocktakeTaskCreator AutoStocktakeTaskCreator { get; }
		Lazy<IWhsAutoInventoryAccuracyManagementTaskCreator> TaskCreator { get; }

		public void CreateAutoInventoryAccuracyManagementTasks(IReadOnlyCollection<WhsPickAvailableInventory> availableInventories, WhsWarehouse warehouse)
		{
			if (warehouse != null)
			{
				AutoInventoryAccuracyManagementTaskCreator.BeforeAutoTasksCreation(availableInventories, warehouse.Factory);

				CreateAutoInventoryAccuracyManagementTasksForAutoTouchCount(availableInventories, warehouse);
				CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(availableInventories, warehouse);
				ResetTouchCountForAndReturnLocationsConfirmedEmpty(availableInventories);
			}
		}

		void CreateAutoInventoryAccuracyManagementTasksForAutoTouchCount(IEnumerable<WhsPickAvailableInventory> availableInventories, WhsWarehouse warehouse)
		{
			var groupedAvailableInventoryLines = availableInventories.Where(l => l.Allocate && l.Location.IsMaximumTouchCountUsed).GroupBy(l => l.Location);
			var locationsWithTouchCountReset = new List<IGrouping<WhsLocation, WhsPickAvailableInventory>>();

			foreach (var line in groupedAvailableInventoryLines)
			{
				var touchCountWasReset = line.Key.UpdateCurrentTouchCount();
				if (touchCountWasReset)
				{
					locationsWithTouchCountReset.Add(line);
				}
			}

			if (locationsWithTouchCountReset.Count > 0)
			{
				LocationsForRollingBack.UnionWith(locationsWithTouchCountReset.Select(l => l.Key));
				AutoInventoryAccuracyManagementTaskCreator.CreateInventoryAccuracyManagementTasksForAutoTouchCount(locationsWithTouchCountReset, warehouse);
			}
		}

		void CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(IEnumerable<WhsPickAvailableInventory> availableInventories, WhsWarehouse warehouse)
		{
			if (warehouse != null && warehouse.WW_VerifyEmptyLocations)
			{
				var groupedAvailableInventoryLines = availableInventories.Where(l => l.Allocate && l.IsLocationEmptyAfterFinalisingPick && l.VerifiedNonEmpty).GroupBy(l => l.Location);
				LocationsForRollingBack.UnionWith(groupedAvailableInventoryLines.Select(l => l.Key));
				AutoInventoryAccuracyManagementTaskCreator.CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(groupedAvailableInventoryLines, warehouse);
			}
		}

		void ResetTouchCountForAndReturnLocationsConfirmedEmpty(IEnumerable<WhsPickAvailableInventory> availableInventoryLines)
		{
			var availableInventoryLinesToResetTouchCount = availableInventoryLines.Where(l => l.Allocate && l.Location.IsMaximumTouchCountUsed);
			var groupedLines = availableInventoryLinesToResetTouchCount
				.GroupBy(line => line.Location)
				.Select(groupedLine => new
				{
					Location = groupedLine.Key,
					IsLocationEmptyAfterFinalisingPick = groupedLine.All(l => l.IsLocationEmptyAfterFinalisingPick),
					VerifiedEmpty = groupedLine.All(l => !l.VerifiedNonEmpty)
				});

			foreach (var groupedLine in groupedLines)
			{
				if (groupedLine.IsLocationEmptyAfterFinalisingPick && groupedLine.VerifiedEmpty)
				{
					groupedLine.Location.WLV_FinalisedPickCount = 0;
					LocationsForRollingBack.Add(groupedLine.Location);
				}
			}
		}

		public void ClearAutoCreatedTasks(bool requireRollback)
		{
			ClearUpdatedLocations();
			AutoInventoryAccuracyManagementTaskCreator.ClearAutoCreatedTasks(requireRollback);

			void ClearUpdatedLocations()
			{
				if (requireRollback)
				{
					foreach (var location in LocationsForRollingBack)
					{
						location.WLV_FinalisedPickCount = (ZInt)location.WLV_FinalisedPickCountInfo.OriginalValue;
					}
				}

				LocationsForRollingBack.Clear();
			}
		}

		IWhsAutoInventoryAccuracyManagementTaskCreator AutoInventoryAccuracyManagementTaskCreator => TaskCreator.Value;

		HashSet<WhsLocation> LocationsForRollingBack => locationsForRollingBack ?? (locationsForRollingBack = new HashSet<WhsLocation>());
		HashSet<WhsLocation> locationsForRollingBack;
	}
}
