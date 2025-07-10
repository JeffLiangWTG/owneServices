using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class LocationProductPalletValidationCache
	{
		public LocationProductPalletValidationCache(BusinessObjectFactory factory, WhsDocketLine[] lines, ZGuid docketPKToExclude)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(lines, nameof(lines));

			var locationPKs = lines.Select(l => l.WE_WL).Distinct().ToArray();
			var palletIDsOnThisJob = lines.Select(l => l.WE_PalletID.ToUpper()).Distinct().ToArray();

			LocationInfoCache =
				new Lazy<IReadOnlyDictionary<ZGuid, (ZInt PalletCount, ZInt RequiredPalletCount, ZBool UsesMoreThanOneProduct)>>(() =>
				{
					var resultSet = new DynamicBusinessObjectCollection(factory);
					var rawSql = @$"
WITH CurrentStock AS
(
	SELECT WE_WL,
		WE_OP,
		WE_PalletID,
		Value
	FROM dbo.WhsDocketLine
		LEFT JOIN @PalletIDsToExclude ON WE_PalletID <> '' AND WE_PalletID = Value
	WHERE WE_WL IN (SELECT Value FROM @LocationPKs)
		AND WE_StockOnHand > 0
		AND WE_DocketLineStatus = 'FIN'
		AND ISNULL(@DocketToExclude, '00000000-0000-0000-0000-000000000000') <> WE_WD
),
PendingStock AS
(
	SELECT WE_WL,
		WE_OP,
		WE_PalletID,
		Value
	FROM dbo.WhsDocketLine
		LEFT JOIN @PalletIDsToExclude ON WE_PalletID <> '' AND WE_PalletID = Value
	WHERE WE_WL IN (SELECT Value FROM @LocationPKs)
		AND WE_TransactionQuantity > 0
		AND WE_DocketLineStatus NOT IN ('FIN', 'PFU', 'CAN')
		AND WE_DocketLineType <> 'ORD'
		AND ISNULL(@DocketToExclude, '00000000-0000-0000-0000-000000000000') <> WE_WD
),
AllStock AS
(
	SELECT WE_WL,
		WE_OP,
		WE_PalletID,
		Value
	FROM CurrentStock

	UNION ALL

	SELECT WE_WL,
		WE_OP,
		WE_PalletID,
		Value
	FROM PendingStock
)
SELECT WE_WL,
	PalletCount = COUNT(DISTINCT CASE WHEN WE_PalletID <> '' AND Value IS NULL THEN WE_PalletID END),
	FirstProductPK = MAX(WE_OP)
FROM AllStock
GROUP BY WE_WL
";
					resultSet.Load(rawSql, new []
					{
						ZSqlParameter.New("@LocationPKs", locationPKs, WhsDocketLineSchema.WE_WL, true),
						ZSqlParameter.New("@DocketToExclude", docketPKToExclude, WhsDocketLineSchema.WE_WD),
						ZSqlParameter.New("@PalletIDsToExclude", palletIDsOnThisJob, WhsDocketLineSchema.WE_PalletID, true),
					});

					var linesLocationLookup = lines.ToLookup(l => l.WE_WL);
					var resultDict = resultSet.ToDictionary
					(
						r => (ZGuid)r["WE_WL"],
						r => ((ZInt)r["PalletCount"],
							CalculateRequiredLocationPalletCount(linesLocationLookup, (ZGuid)r["WE_WL"]),
							CalculateLocationUsesMoreThanOneProduct(linesLocationLookup, (ZGuid)r["WE_WL"], (ZGuid)r["FirstProductPK"]))
					);

					locationPKs.ForEach(
						locationPK => resultDict.GetOrAdd(locationPK, () =>
							(
								0,
								CalculateRequiredLocationPalletCount(linesLocationLookup, locationPK),
								CalculateLocationUsesMoreThanOneProduct(linesLocationLookup, locationPK, ZGuid.Empty)
							)
						)
					);
					return resultDict;
				});
		}

		Lazy<IReadOnlyDictionary<ZGuid, (ZInt PalletCount, ZInt RequiredPalletCount, ZBool UsesMoreThanOneProduct)>> LocationInfoCache { get; }

		public int GetPalletCountExcludingThisJob(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase ? LocationInfoCache.Value[location.PK].PalletCount : 0;
		}

		public ZInt GetRequiredLocationPalletCount(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase ? LocationInfoCache.Value[location.PK].RequiredPalletCount : 0;
		}

		public ZBool GetLocationUsesMoreThanOneProduct(WhsLocation location)
		{
			Argument.NotNull(location, nameof(location));
			return location.IsInDatabase ? LocationInfoCache.Value[location.PK].UsesMoreThanOneProduct : ZBool.False;
		}

		ZInt CalculateRequiredLocationPalletCount(ILookup<ZGuid, WhsDocketLine> linesLocationLookup, ZGuid locationPK)
		{
			return linesLocationLookup[locationPK].Where(l => !l.WE_PalletID.IsEmpty)
				.Select(l => l.WE_PalletID.ToUpper()).Distinct().Count();
		}

		ZBool CalculateLocationUsesMoreThanOneProduct(ILookup<ZGuid, WhsDocketLine> linesLocationLookup, ZGuid locationPK, ZGuid availableFirstProductPK)
		{
			var firstTwoProducts = linesLocationLookup[locationPK].Where(l => !l.WE_OP.IsEmpty).Select(l => l.WE_OP).Distinct().Take(2).ToArray();
			return firstTwoProducts.Length > 1 || (availableFirstProductPK != ZGuid.Empty && firstTwoProducts[0] != availableFirstProductPK);
		}
	}
}
