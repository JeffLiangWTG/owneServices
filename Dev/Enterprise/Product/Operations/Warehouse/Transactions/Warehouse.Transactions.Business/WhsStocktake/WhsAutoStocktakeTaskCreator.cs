using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAutoStocktakeTaskCreator : IWhsAutoStocktakeTaskCreator
	{
		public void BeforeAutoTasksCreation(IReadOnlyCollection<WhsPickAvailableInventory> availableInventories, BusinessObjectFactory factory)
		{
			if (availableInventories.Count > 0)
			{
				var query = new ZQuery();
				query.AddToFilter(WhsStocktakeLineSchema.WU_WL, availableInventories.Select(l => l.LocationPK).Distinct());
				query.AddToFilter(WhsStocktakeLineSchema.WU_Status, StocktakeLineStatus.Codes.Open);
				factory.Load<WhsStocktakeLine>(query); // Load stocktake lines into memory
			}
		}

		public void CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>> groupedAvailableInventoryLines, WhsWarehouse warehouse)
		{
			groupedAvailableInventoryLines.ForEach(l => CreateStocktakes(StocktakeTypeCodeList.Codes.AutomaticZeroConfirmation, l, false, warehouse));
		}

		public void CreateInventoryAccuracyManagementTasksForAutoTouchCount(IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>> groupedAvailableInventoryLines, WhsWarehouse warehouse)
		{
			var clientsByLocation = LoadClientsByLocations(groupedAvailableInventoryLines.Select(l => l.Key), warehouse.Factory);
			foreach (var location in groupedAvailableInventoryLines)
			{
				CreateStocktakes(StocktakeTypeCodeList.Codes.AutomaticTouchCount, location, true, warehouse, clientsByLocation);
			}
		}

		Dictionary<ZGuid, IEnumerable<ZGuid>> LoadClientsByLocations(IEnumerable<WhsLocation> locations, BusinessObjectFactory factory)
		{
			Dictionary<ZGuid, IEnumerable<ZGuid>> result;
			var locationPKs = locations.Select(l => l.PK).ToArray();
			if (locationPKs.Length > 0)
			{
				var rawSqlQuery = $@"
SELECT DISTINCT
	WE_WL,
	WD_OH_Client
FROM
	dbo.WhsDocketLine
	JOIN dbo.WhsDocket ON WD_PK = WE_WD
WHERE
	WE_DocketLineStatus = '{DocketLineStatus.Codes.Finalised}'
	AND WE_WL IN (SELECT value FROM @LocationPKs)
	AND WE_StockOnHand > 0
GROUP BY
	WE_WL,
	WD_OH_Client";

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add(ZSqlParameter.New("@LocationPKs", locationPKs, WhsDocketLineSchema.WE_WL, isTableValued: true));

				var locationsAndClients = new DynamicBusinessObjectCollection(factory);
				locationsAndClients.Load(rawSqlQuery, sqlParams);

				result = locationsAndClients
					.GroupBy(row => (ZGuid)row[WhsDocketLineSchema.WE_WL])
					.ToDictionary(group => group.Key, group => group.Select(row => (ZGuid)row[WhsDocketSchema.WD_OH_Client]));
			}
			else
			{
				result = new Dictionary<ZGuid, IEnumerable<ZGuid>>();
			}

			return result;
		}

		void CreateStocktakes(string stocktakeType, IGrouping<WhsLocation, WhsPickAvailableInventory> linesByLocation, bool loadInventory, WhsWarehouse warehouse, Dictionary<ZGuid, IEnumerable<ZGuid>> clientsByLocations = null)
		{
			var availableInventoryByClients = linesByLocation.GroupBy(l => l.Client.PK);
			foreach (var availableInventoryByClient in availableInventoryByClients)
			{
				var (stocktake, lines) = SetupAndLoadStocktake(stocktakeType, availableInventoryByClient.Key, linesByLocation.Key, warehouse, loadInventory);
				if (stocktake != null)
				{
					foreach (var availableInventoryLine in availableInventoryByClient.Distinct())
					{
						var matchingLine = MatchingStocktakeLineExistsInStocktake(lines, availableInventoryLine);
						if (loadInventory && matchingLine != null)
						{
							// If user set PickedDateTime and Save into DB before FinalisePick, we should not subtract the units in Stocktake again.
							var picklineQuantity = availableInventoryLine.PickLines.Where(pl => pl.WasPickedInMemoryForStocktake).Sum(p => p.WZ_Units);
							matchingLine.WU_SystemUnits -= picklineQuantity;
						}
						else
						{
							var line = stocktake.Lines.AddNew();
							line.WU_OP = availableInventoryLine.SupplierPart.PK;
							line.WU_PartAttrib1 = availableInventoryLine.PartAttrib1;
							line.WU_PartAttrib2 = availableInventoryLine.PartAttrib2;
							line.WU_PartAttrib3 = availableInventoryLine.PartAttrib3;
							line.WU_SerialNumber = availableInventoryLine.SerialNumber;
							line.WU_PackingDate = availableInventoryLine.PackingDate;
							line.WU_ExpiryDate = availableInventoryLine.ExpiryDate;
							line.WU_IsManuallyAdded = false;
						}
					}
				}
			}

			IEnumerable<ZGuid> existingClients = null;
			if (clientsByLocations != null && clientsByLocations.TryGetValue(linesByLocation.Key.PK, out existingClients))
			{
				var clientsNotInThisPick = existingClients.Except(availableInventoryByClients.Select(l => l.Key).Distinct());
				foreach (var client in clientsNotInThisPick)
				{
					SetupAndLoadStocktake(stocktakeType, client, linesByLocation.Key, warehouse, true); //Load stocktakes for clients which are not part of this pick
				}
			}
		}

		(WhsStocktake, IEnumerable<WhsStocktakeLine>) SetupAndLoadStocktake(string stocktakeType, ZGuid client, WhsLocation location, WhsWarehouse warehouse, bool loadInventory)
		{
			WhsStocktake stocktake = null;
			IEnumerable<WhsStocktakeLine> lines = new List<WhsStocktakeLine>();

			var factory = location.Factory;

			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			query.AddToFilter(WhsStocktakeLineSchema.WU_OH_Client, client);
			query.AddToFilter(WhsStocktakeLineSchema.WU_WL, location.PK);
			query.AddToFilter(WhsStocktakeLineSchema.WU_Status, StocktakeLineStatus.Codes.Open);
			var stocktakeLines = factory.Load<WhsStocktakeLine>(query);

			if (!(stocktakeLines.Length > 0))
			{
				stocktake = factory.New<WhsStocktake>();
				stocktake.IsAutoCreatingStocktake = true;
				stocktake.WS_StocktakeType = stocktakeType;
				stocktake.WS_WW_Whs = warehouse.PK;
				stocktake.WS_OH_Client = client;
				stocktake.WS_WL_Location = location.PK;

				if (loadInventory)
				{
					lines = stocktake.CreateStocktakeLines();
				}
				stocktake.Lines.AddRange(lines);

				stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Loaded;
				StocktakesForRollingBack.Add(stocktake);
			}

			return (stocktake, lines);
		}

		WhsStocktakeLine MatchingStocktakeLineExistsInStocktake(IEnumerable<WhsStocktakeLine> lines, WhsPickAvailableInventory availableInventoryLine)
		{
			WhsStocktakeLine result = null;
			if (lines != null)
			{
				var inventory = availableInventoryLine.Inventory.Count > 0 ? availableInventoryLine.Inventory[0] : null;
				result = lines.SingleOrDefault(l => l.WU_InventoryStatus.Equals(InventoryStatus.Codes.Available) && // availInventoryLine is 'AVL' only, no need to compare other types
					l.WU_OP == availableInventoryLine.SupplierPart.PK &&
					l.IsMatchingStocktakeLine(availableInventoryLine, availableInventoryLine.PalletID,
						inventory?.PackageGroupId ?? ZString.Empty,
						inventory?.PerPackageQty ?? ZDecimal.Zero
						));
			}
			return result;
		}

		public void ClearAutoCreatedTasks(bool requireRollback)
		{
			if (requireRollback)
			{
				foreach (var stocktake in StocktakesForRollingBack)
				{
					stocktake.Delete();
				}
			}

			StocktakesForRollingBack.Clear();
		}

		List<WhsStocktake> StocktakesForRollingBack => stocktakesForRollingBack ?? (stocktakesForRollingBack = new List<WhsStocktake>());
		List<WhsStocktake> stocktakesForRollingBack;
	}
}
