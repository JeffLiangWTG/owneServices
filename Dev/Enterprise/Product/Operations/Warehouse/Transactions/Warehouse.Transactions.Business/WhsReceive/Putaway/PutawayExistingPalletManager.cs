using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PutawayExistingPalletManager : IPutawayExistingPalletManager
	{
		#region AllocateExistingPalletLocations

		public IEnumerable<WhsInventoryView> AllocateExistingPalletLocations(BusinessObjectFactory factory, IEnumerable<WhsReceive> receives, IEnumerable<WhsReceiveLine> lines, WhsWarehouse warehouse)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(receives, nameof(receives));
			Argument.NotNull(lines, nameof(lines));
			Argument.NotNull(warehouse, nameof(warehouse));

			var inventoriesToPutawayOnOtherReceives = Enumerable.Empty<WhsInventoryView>();

			var putawayPalletInventory = receives.SelectMany(r => r.Lines.Cast<ILineToPutaway>().Where(i => i.QuantityToPutaway > 0 && !i.PalletID.IsEmpty && !i.LocationPK.IsEmpty)).ToArray();
			var palletLinesNotPutaway = lines.Cast<ILineToPutaway>().Where(i => !i.PalletID.IsEmpty && i.LocationPK.IsEmpty).ToList();

			if (palletLinesNotPutaway.Count > 0)
			{
				var putawayPalletsOnThisReceiveDictionary = new Dictionary<string, ZGuid>(StringComparer.OrdinalIgnoreCase);
				foreach (var putawayPallet in putawayPalletInventory)
				{
					putawayPalletsOnThisReceiveDictionary[putawayPallet.PalletID] = putawayPallet.LocationPK;
				}

				SetLocationOnExistingPallets(factory, palletLinesNotPutaway, putawayPalletsOnThisReceiveDictionary);

				if (palletLinesNotPutaway.Count > 0)
				{
					var palletIDs = palletLinesNotPutaway.Select(i => i.PalletID).Distinct();

					const bool hasDestinationLocationSet = false;
					var inventoriesUsingSamePalletID = GetInventoryUsingSamePalletID(factory, receives.Select(r => r.PK).ToArray(), palletIDs, warehouse).ToLookup(i => i.WE_WL.IsEmpty || i.WE_CurrentInventoryStatus == InventoryStatus.Codes.Received);
					var inventoriesUsingSamePalletIDWithLocations = inventoriesUsingSamePalletID[hasDestinationLocationSet];

					var putawayPalletsDictionary = new Dictionary<string, ZGuid>(StringComparer.OrdinalIgnoreCase);
					foreach (var putawayPallet in IEnumerableExtensions.DistinctBy(inventoriesUsingSamePalletIDWithLocations, i => i.WE_PalletID))
					{
						putawayPalletsDictionary[putawayPallet.WE_PalletID] = putawayPallet.WE_WL;
					}

					SetLocationOnExistingPallets(factory, palletLinesNotPutaway, putawayPalletsDictionary);

					var docketLinesToPutawayOnOtherReceives = inventoriesUsingSamePalletID[!hasDestinationLocationSet].Where(i => !putawayPalletsDictionary.ContainsKey(i.WE_PalletID)).ToArray();
					docketLinesToPutawayOnOtherReceives.ForEach(dl => factory.AddFetchHint(WhsInventoryViewSchema.WI_WE_InDocketLine, dl.PK));
					inventoriesToPutawayOnOtherReceives = docketLinesToPutawayOnOtherReceives.Select(i => i.Inventory[0]);
				}
			}

			return inventoriesToPutawayOnOtherReceives;
		}

		static IEnumerable<WhsDocketLine> GetInventoryUsingSamePalletID(BusinessObjectFactory factory, ZGuid[] receivePKs, IEnumerable<ZString> palletIDs, WhsWarehouse warehouse)
		{
			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
			docketSubQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehouse.PK);

			var docketLineQuery = new ZDBOnlyQuery(typeof(WhsDocketLine));
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_PalletID, palletIDs);
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, SQLComparisonOperator.NotEqual, receivePKs);
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);

			docketLineQuery.AddSubQuery(docketSubQuery, JoinCondition.And);

			var putawayTransferNotForCurrentReceiveQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_InventoryLine);
			putawayTransferNotForCurrentReceiveQuery.AddToFilter(WhsDocketLineSchema.WE_WD, receivePKs);

			var putawayTransferPickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine, notIn: true);
			putawayTransferPickLineSubQuery.AddSubQuery(putawayTransferNotForCurrentReceiveQuery, JoinCondition.And);

			docketLineQuery.AddSubQuery(putawayTransferPickLineSubQuery, JoinCondition.And);

			var docketLineSubQueryForFetchHint = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			docketLineSubQueryForFetchHint.AddToFilter(docketLineQuery);

			var docketQueryForFetchHint = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQueryForFetchHint.AddSubQuery(docketLineSubQueryForFetchHint, JoinCondition.And);
			factory.AddFetchHint(WhsDocketSchema.Instance, docketQueryForFetchHint);

			var inventoriesUsingSamePalletID = factory.Load<WhsDocketLine>(docketLineQuery);
			return inventoriesUsingSamePalletID;
		}

		static void SetLocationOnExistingPallets(BusinessObjectFactory factory, List<ILineToPutaway> palletLinesNotPutaway, Dictionary<string, ZGuid> putawayPallets)
		{
			foreach (var kvp in putawayPallets)
			{
				factory.AddFetchHint(WhsDocketLineSchema.Instance, WhsValidationHelper.GetLocationsWithStockIncludingNotYetFinalisedQuery(kvp.Key));
			}

			for (int i = palletLinesNotPutaway.Count - 1; i >= 0; i--)
			{
				var inventoryLine = palletLinesNotPutaway[i];

				if (putawayPallets.TryGetValue(inventoryLine.PalletID, out var locationPK))
				{
					inventoryLine.LocationPK = locationPK;
					palletLinesNotPutaway.RemoveAt(i);
				}
			}
		}

		#endregion
	}
}
