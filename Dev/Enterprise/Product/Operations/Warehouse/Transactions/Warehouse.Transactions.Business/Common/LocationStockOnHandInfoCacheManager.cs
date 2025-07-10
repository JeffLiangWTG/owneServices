using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	internal class LocationStockOnHandInfoCacheManager
	{
		public static IEnumerable<LocationStockOnHandInfo> GetLocationStockOnHandCache(BusinessObjectFactory factory, ZGuid locationPK, ZGuid docketPK)
		{
			var key = "LocationStockOnHandInfoCache|" + docketPK + "|" + locationPK;
			return factory.GetCachedValue(key, () => GetLocationStockOnHandInfos(factory, locationPK, docketPK), CacheStalenessPolicy.StaleOnFactorySave);
		}

		static IEnumerable<LocationStockOnHandInfo> GetLocationStockOnHandInfos(BusinessObjectFactory factory, ZGuid locationPK, ZGuid docketPK)
		{
			var sql = $@"
SELECT DISTINCT WD_PK, WE_PalletID, OH_Code, OH_PK, OP_PartNum, OP_PK
FROM
(
	SELECT
		WD_PK,
		WE_PalletID,
		OH_Code,
		OH_PK,
		OP_PartNum,
		OP_PK
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
		JOIN dbo.OrgHeader ON WD_OH_Client = OH_PK
		JOIN dbo.OrgSupplierPart ON WE_OP = OP_PK
	WHERE
		WE_StockOnHand > 0
		AND WE_DocketLineStatus = '{DocketLineStatus.Codes.Finalised}'
		AND WE_WL = @LocationPK
		AND WD_PK <> @DocketPK

	UNION ALL 

	SELECT
		WD_PK,
		WE_PalletID,
		OH_Code,
		OH_PK,
		OP_PartNum,
		OP_PK
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
		JOIN dbo.OrgHeader ON WD_OH_Client = OH_PK
		JOIN dbo.OrgSupplierPart ON WE_OP = OP_PK
	WHERE
		WE_TransactionQuantity > 0
		AND WE_DocketLineStatus <> '{DocketLineStatus.Codes.Finalised}'
		AND WE_WL = @LocationPK
		AND WD_PK <> @DocketPK
) LocationStockOnHandInfos
";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@DocketPK", docketPK, WhsDocketLineSchema.WE_WD);
			sqlParams.Add("@LocationPK", locationPK, WhsDocketLineSchema.WE_WL);

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, sqlParams);

			return collection
				.Select(bizo => new LocationStockOnHandInfo(
					(ZGuid)bizo[WhsDocketSchema.PK],
					(ZString)bizo[WhsDocketLineSchema.WE_PalletID],
					(ZString)bizo[OrgHeaderSchema.OH_Code],
					(ZGuid)bizo[OrgHeaderSchema.PK],
					(ZString)bizo[OrgSupplierPartSchema.OP_PartNum],
					(ZGuid)bizo[OrgSupplierPartSchema.PK]));
		}
	}
}
