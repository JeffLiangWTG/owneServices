using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickOnSalesOrderDetector : IWhsPickOnSalesOrderDetector
	{
		public bool IsComponentUsedToBuiltKitOnSalesOrder(BusinessObjectFactory factory, ZGuid productPK)
		{
			return factory.GetCachedValue("IsComponentUsedToBuiltKitOnSalesOrder|" + productPK, () =>
			{
				var unfinalisedRecord = new DynamicBusinessObjectCollection(factory);
				var sql = @"
SELECT
	TOP 1 WP_PK
FROM
	dbo.WhsPick
	JOIN dbo.WhsDocket ON WD_WP = WP_PK
	JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
WHERE
	WP_PickStatus <> 'FIN'
	AND WP_PickStatus <> 'CAN'
	AND WE_DocketLineType = 'ORD'
	AND WE_OP = @ProductPK
	AND WE_WE_ParentDocketLine IS NOT NULL

UNION ALL

-- This part can be deleted after the removal of the Transformation PopulateWhsPickByBOMData.cs
SELECT
	TOP 1 WP_PK
FROM
	dbo.WhsPick
	JOIN dbo.WhsDocket ON WD_WP = WP_PK
	JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
WHERE
	WP_FinalizedDateUtc IS NOT NULL
	AND WE_DocketLineType = 'ORD'
	AND WE_WE_ParentDocketLine IS NOT NULL
	AND WE_OP = @ProductPK
	AND NOT EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.WhsDocket ReceiveFromPick
		WHERE
			ReceiveFromPick.WD_WP_ParentPickForReceive = WP_PK
	)
";
				var parameters = new ZSqlParameterCollection
				{
					ZSqlParameter.New("@ProductPK", productPK, WhsDocketLineSchema.WE_OP)
				};
				unfinalisedRecord.Load(sql, parameters);

				return unfinalisedRecord.Count > 0;
			});
		}

		public bool IsKitBuiltOnSalesOrder(BusinessObjectFactory factory, ZGuid productPK)
		{
			return factory.GetCachedValue("IsKitBuiltOnSalesOrder|" + productPK, () =>
			{
				var unfinalisedRecord = new DynamicBusinessObjectCollection(factory);
				var sql = @"
SELECT
	TOP 1 WP_PK
FROM
	dbo.WhsPick
	JOIN dbo.WhsDocket ON WP_PK = WD_WP
	JOIN dbo.WhsDocketLine ParentLine ON WD_PK = ParentLine.WE_WD
	JOIN dbo.WhsDocketLine ComponentLine ON ComponentLine.WE_WE_ParentDocketLine = ParentLine.WE_PK
WHERE
	WP_PickStatus <> 'FIN'
	AND WP_PickStatus <> 'CAN'
	AND ParentLine.WE_DocketLineType = 'ORD'
	AND ParentLine.WE_OP = @ProductPK

UNION ALL

-- This part can be deleted after the removal of the Transformation PopulateWhsPickByBOMData.cs
SELECT
	TOP 1 WP_PK
FROM
	dbo.WhsPick
	JOIN dbo.WhsDocket ON WP_PK = WD_WP
	JOIN dbo.WhsDocketLine ParentLine ON WD_PK = ParentLine.WE_WD AND ParentLine.WE_WE_ParentDocketLine IS NULL
	JOIN dbo.WhsDocketLine ComponentLine ON ComponentLine.WE_WE_ParentDocketLine = ParentLine.WE_PK
WHERE
	WP_FinalizedDateUtc IS NOT NULL
	AND ParentLine.WE_DocketLineType = 'ORD'
	AND ParentLine.WE_OP = @ProductPK
	AND NOT EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.WhsDocket ReceiveFromPick
		WHERE
			ReceiveFromPick.WD_WP_ParentPickForReceive = WP_PK
	)
";
				var parameters = new ZSqlParameterCollection
				{
					ZSqlParameter.New("@ProductPK", productPK, WhsDocketLineSchema.WE_OP)
				};
				unfinalisedRecord.Load(sql, parameters);

				return unfinalisedRecord.Count > 0;
			});
		}
	}
}
