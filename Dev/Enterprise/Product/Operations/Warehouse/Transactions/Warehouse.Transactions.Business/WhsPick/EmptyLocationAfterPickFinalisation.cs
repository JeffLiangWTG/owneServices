using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IEmptyLocationAfterPickFinalisation
	{
		bool IsLocationEmptyAfterFinalisingPick { get; set; }
		decimal AllocatedQty { get; }
		WhsDocketLine Inventory { get; }
	}

	public static class UpdatePickLinesWithEmptyLocationsExtensions
	{
		public static void UpdateIsLocationEmptyAfterFinalisingPickOnPickLines<T>(this WhsPick pick, IEnumerable<T> lines)
			where T : IEmptyLocationAfterPickFinalisation
		{
			var warehouse = pick.Warehouse;
			var verifyLocationIsOn = warehouse != null && warehouse.WW_VerifyEmptyLocations;
			if (verifyLocationIsOn)
			{
				var linesWithKey = lines.Select(l => new { Key = LocationProductAndClient.New(l.Inventory), l.AllocatedQty, PickLine = l }).ToArray();
				if (linesWithKey.Length > 0)
				{
					var inventoryLocationAndProductGuids = LoadLocationAndProductGuidsOnlyFromInventory(pick, linesWithKey.Select(l => l.Key).ToArray());

					var locationAndProductGuidsFromPickLines = linesWithKey
						.GroupBy(line => line.Key)
						.ToDictionary(group => group.Key, group => group.Sum(l => l.AllocatedQty));

					foreach (var line in linesWithKey)
					{
						inventoryLocationAndProductGuids.TryGetValue(line.Key, out var inventoryQty);
						locationAndProductGuidsFromPickLines.TryGetValue(line.Key, out var pickedQty);

						line.PickLine.IsLocationEmptyAfterFinalisingPick = inventoryQty == pickedQty;
					}
				}
			}
		}

		static IDictionary<LocationProductAndClient, ZDecimal> LoadLocationAndProductGuidsOnlyFromInventory(WhsPick pick, IReadOnlyCollection<LocationProductAndClient> locationProductAndClientGuids)
		{
			var rawSqlQuery = String.Format(Culture.Invariant, @"
				SELECT WI_WL, WI_OP, WI_OH_Client, SUM(WI_TotalUnits) AS TotalUnits
				FROM dbo.WhsInventoryView
				WHERE WI_WL IN ('{0}') AND WI_OP IN ('{1}') AND WI_OH_Client IN ('{2}') AND WI_TotalUnits > 0 AND WI_InventoryStatus NOT IN (@InTransitStatus, @PuttingAwayStatus)
				GROUP BY WI_WL, WI_OP, WI_OH_Client",
	string.Join("','", locationProductAndClientGuids.Select(o => o.Location)),
	string.Join("','", locationProductAndClientGuids.Select(o => o.Product)),
	string.Join("','", locationProductAndClientGuids.Select(o => o.Client)));

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@InTransitStatus", InventoryStatus.Codes.InTransit, WhsInventoryViewSchema.WI_InventoryStatus);
			sqlParams.Add("@PuttingAwayStatus", InventoryStatus.Codes.PuttingAway, WhsInventoryViewSchema.WI_InventoryStatus);
			var locationsAndProducts = new DynamicBusinessObjectCollection(pick.Factory);
			locationsAndProducts.Load(rawSqlQuery, sqlParams);

			var locationsProductsAndClients = new Dictionary<LocationProductAndClient, ZDecimal>();

			foreach (DynamicBusinessObject row in locationsAndProducts)
			{
				locationsProductsAndClients.Add(new LocationProductAndClient(
					(ZGuid)row[WhsInventoryViewSchema.WI_WL],
					(ZGuid)row[WhsInventoryViewSchema.WI_OP],
					(ZGuid)row[WhsInventoryViewSchema.WI_OH_Client]),
					(ZDecimal)row["TotalUnits"]);
			}

			return locationsProductsAndClients;
		}

		class LocationProductAndClient : IEquatable<LocationProductAndClient>
		{
			public LocationProductAndClient(ZGuid location, ZGuid product, ZGuid client)
			{
				Location = location;
				Product = product;
				Client = client;
			}

			public ZGuid Location { get; }
			public ZGuid Product { get; }
			public ZGuid Client { get; }

			public static LocationProductAndClient New(WhsDocketLine inventoryLine) => new LocationProductAndClient(inventoryLine.WE_WL, inventoryLine.WE_OP, inventoryLine.Docket.WD_OH_Client);

			#region Equals

			public override bool Equals(object obj)
			{
				var other = obj as LocationProductAndClient;
				return other != null && ((IEquatable<LocationProductAndClient>)this).Equals(other);
			}

			#endregion

			#region GetHashCode

			public override int GetHashCode()
			{
				return Location.GetHashCode() ^ Product.GetHashCode() ^ Client.GetHashCode();
			}

			#endregion

			#region IEquatable<LocationProductAndClient> Members

			bool IEquatable<LocationProductAndClient>.Equals(LocationProductAndClient other)
			{
				return (Location == other.Location) && (Product == other.Product) && Client == other.Client;
			}

			#endregion
		}
	}
}
