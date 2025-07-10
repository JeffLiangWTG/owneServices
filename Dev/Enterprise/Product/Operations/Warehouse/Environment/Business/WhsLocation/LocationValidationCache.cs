using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	class LocationValidationCache
	{
		public LocationValidationCache(BusinessObjectFactory factory, WhsRow row, IEnumerable<WhsLocation> locationsToCheck)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(locationsToCheck, nameof(locationsToCheck));
			Argument.NotNull(row, nameof(row));

			CheckDigitMapping = new Lazy<Dictionary<ZByte, HashSet<ZGuid>>>(() =>
				row.Locations
					.Where(location => location.WLV_CheckDigit != WhsLocation.EmptyCheckDigit)
					.GroupBy(location => location.WLV_CheckDigit)
					.ToDictionary(group => group.Key, group => group.Select(l => l.PK).ToHashSet()));

			LocationInfoCache = new Lazy<IReadOnlyDictionary<ZGuid, (bool HasDockDoorPick, bool HasPackingStationPick, bool HasUnfinalisedPick, bool HasPickFace, bool HasNonPalletStock, int PalletCount, int ProductCount)>>(() =>
			{
				var locationPKs = locationsToCheck.Where(l => l.IsInDatabase).Select(l => l.PK).ToArray();
				var resultSet = new DynamicBusinessObjectCollection(factory);
				resultSet.Load(@"
SELECT
	WL_PK,
	CAST(CASE WHEN DockDoorPick IS NOT NULL OR DockDoorAssignment IS NOT NULL THEN 1 ELSE 0 END as bit) as HasDockDoorPick,
	CAST(CASE WHEN PackingStationPick IS NOT NULL THEN 1 ELSE 0 END as bit) as HasPackingStationPick,
	CAST(CASE WHEN UnfinalisedPick IS NOT NULL THEN 1 ELSE 0 END as bit) as HasUnfinalisedPick,
	CAST(CASE WHEN PickFace IS NOT NULL THEN 1 ELSE 0 END as bit) as HasPickFace,
	CAST(CASE WHEN HasNonPalletStock = 1 THEN 1 ELSE 0 END as bit) as HasNonPalletStock,
	PalletCount,
	ProductCount
FROM
	dbo.WhsLocation
	OUTER APPLY
	(
		SELECT TOP 1 WP_PK as DockDoorPick
		FROM dbo.WhsPick
		WHERE
			WP_WL_DockDoor = WL_PK
	) as DockDoorPick
	OUTER APPLY
	(
		SELECT TOP 1 WDA_PK as DockDoorAssignment
		FROM dbo.WhsDockDoorAssignment
		WHERE
			WDA_WL_AssignedDockDoor = WL_PK
	) as DockDoorAssignment
	OUTER APPLY
	(
		SELECT TOP 1 WP_PK as PackingStationPick
		FROM dbo.WhsPick
		WHERE
			WP_WL_PackingStation = WL_PK
	) as PackingStationPick
	OUTER APPLY
	(
		SELECT TOP 1 WP_PK as UnfinalisedPick
		FROM
			dbo.WhsPick
		WHERE
			(
				WP_WL_DockDoor = WL_PK
				OR WP_WL_PackingStation = WL_PK
				OR EXISTS
				(
					SELECT
						NULL
					FROM
						dbo.WhsDockDoorAssignment
					WHERE
						WP_WDA_DockDoorAssignment = WDA_PK
						AND WDA_WL_AssignedDockDoor = WL_PK
				)
			)
			AND WP_PickStatus NOT IN ('CAN', 'FIN')
	) as UnfinalisedPick
	OUTER APPLY
	(
		SELECT TOP 1 WF_PK as PickFace
		FROM dbo.WhsPickFace
		WHERE WF_WL = WL_PK
	) as PickFace
	CROSS APPLY
	(
		SELECT
			MAX(CASE WHEN WE_PalletID = '' THEN 1 ELSE 0 END) AS HasNonPalletStock,
			COUNT(DISTINCT WE_PalletID) AS PalletCount,
			COUNT(DISTINCT WE_OP) AS ProductCount
		FROM
		(
			SELECT
				WE_PalletID,
				WE_OP
			FROM
				dbo.WhsDocketLine 
			WHERE
				WE_WL = WL_PK
				AND WE_StockOnHand > 0
				AND WE_CurrentInventoryStatus NOT IN ('INT', 'PTA')
		) AS Pallets
	) AS Pallets
WHERE
	WL_PK IN (SELECT Value FROM @Locations)
", new[] { ZSqlParameter.New("@Locations", locationPKs, WhsLocationSchema.PK, isTableValued: true) });

				return resultSet.ToDictionary
				(
					r => (ZGuid)r[WhsLocationSchema.Constants.PK],
					r => ((bool)(ZBool)r["HasDockDoorPick"],
					(bool)(ZBool)r["HasPackingStationPick"],
					(bool)(ZBool)r["HasUnfinalisedPick"],
					(bool)(ZBool)r["HasPickFace"],
					(bool)(ZBool)r["HasNonPalletStock"],
					(int)(ZInt)r["PalletCount"],
					(int)(ZInt)r["ProductCount"]
				));
			});
		}

		Lazy<IReadOnlyDictionary<ZGuid, (bool HasDockDoorPick, bool HasPackingStationPick, bool HasUnfinalisedPick, bool HasPickFace, bool HasNonPalletStock, int PalletCount, int ProductCount)>> LocationInfoCache { get; }
		Lazy<Dictionary<ZByte, HashSet<ZGuid>>> CheckDigitMapping { get; }

		public bool HasDockDoorPick(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase && LocationInfoCache.Value[location.PK].HasDockDoorPick;
		}

		public bool HasPackingStationPick(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase && LocationInfoCache.Value[location.PK].HasPackingStationPick;
		}

		public bool HasUnfinalisedPick(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase && LocationInfoCache.Value[location.PK].HasUnfinalisedPick;
		}

		public bool HasPickFace(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase && LocationInfoCache.Value[location.PK].HasPickFace;
		}

		public bool HasNonPalletStock(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase && LocationInfoCache.Value[location.PK].HasNonPalletStock;
		}

		public bool HasNonUniqueCheckDigit(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.WLV_CheckDigit != WhsLocation.EmptyCheckDigit && !CheckDigitCacheSemaphore.IsSuspended && CheckDigitMapping.Value.TryGetValue(location.WLV_CheckDigit, out var pks) && pks.Count > 1;
		}

		public int GetPalletCount(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase ? LocationInfoCache.Value[location.PK].PalletCount : 0;
		}

		public int GetProductCount(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase ? LocationInfoCache.Value[location.PK].ProductCount : 0;
		}

		public IDisposable DeferCheckDigitCacheCreation()
		{
			SemaphoreManager manager = null;
			return new DisposableAction(
				() => manager = new SemaphoreManager(CheckDigitCacheSemaphore),
				() => manager?.Dispose());
		}

		Semaphore CheckDigitCacheSemaphore => checkDigitCacheSemaphore ??= new Semaphore();
		Semaphore checkDigitCacheSemaphore;
	}
}
