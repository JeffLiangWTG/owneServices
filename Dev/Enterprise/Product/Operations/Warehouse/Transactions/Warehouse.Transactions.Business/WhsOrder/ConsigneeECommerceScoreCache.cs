using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	internal sealed class ConsigneeECommerceScoreCache
	{
		public ConsigneeECommerceScoreCache(WhsPick pick)
		{
			Argument.NotNull(pick, nameof(pick));

			ConsigneeCache = new Lazy<HashSet<ZGuid>>(() =>
			{
				var resultSet = new DynamicBusinessObjectCollection(pick.Factory);
				resultSet.Load($@"
SELECT
	WD_PK
FROM
	dbo.WhsDocket
	JOIN dbo.JobDocAddress Consignee ON E2_ParentID = WD_PK AND E2_AddressType = '{DocAddressTypes.Codes.ConsigneeAddress}'
WHERE
	WD_PK IN (SELECT Value FROM @Orders)
	AND Consignee.E2_AddressOverride = 0
	AND EXISTS
	(
		SELECT NULL
		FROM
			dbo.JobDocAddress OtherAddress
		WHERE
			OtherAddress.E2_OA_Address = Consignee.E2_OA_Address
			AND E2_AddressType = '{DocAddressTypes.Codes.ConsigneeAddress}'
			AND OtherAddress.E2_ParentID != WD_PK
			AND OtherAddress.E2_ParentTableCode = '{WhsDocketSchema.Constants.Prefix}'
	)
", new[] { ZSqlParameter.New("@Orders", pick.Orders.Select(o => o.PK).ToArray(), WhsDocketSchema.PK, isTableValued: true) });

				return resultSet.Select(r => (ZGuid)r[WhsDocketSchema.Constants.PK]).ToHashSet();
			});
		}

		Lazy<HashSet<ZGuid>> ConsigneeCache { get; }

		public bool IsConsigneeOnOrderUsedOnOtherWarehouseJobs(WhsOrder order)
		{
			Argument.NotNull(order, nameof(order));
			return ConsigneeCache.Value.Contains(order.PK);
		}
	}
}
