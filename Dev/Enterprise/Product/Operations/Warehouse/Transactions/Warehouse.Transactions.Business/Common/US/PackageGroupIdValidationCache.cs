using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PackageGroupIdsCache
	{
		public PackageGroupIdsCache(BusinessObjectFactory factory, WhsReceive receive)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			Parent = Argument.NotNull(receive, nameof(receive));

			PackageGroupIdInfosCache = new Lazy<HashSet<ZString>>(() =>
			{
				var packingGroupIdsToCheck = Parent.Lines
					.Select(line => line.WE_PackageGroupId.ToUpper())
					.ToArray();

				var resultSet = new DynamicBusinessObjectCollection(Factory);
				resultSet.Load(@"
SELECT
	WE_PackageGroupId
FROM
	dbo.WhsDocketLine
WHERE
	WE_StockOnHand > 0
	AND WE_PackageGroupId <> ''
	AND WE_WD <> @DocketPK
	AND WE_PackageGroupId IN (SELECT Value FROM @PackageGroupIds)
", new[] { ZSqlParameter.New("@PackageGroupIds", packingGroupIdsToCheck, WhsDocketLineSchema.WE_PackageGroupId, isTableValued: true), ZSqlParameter.New("@DocketPK", Parent.PK, WhsDocketLineSchema.WE_WD) });

				return resultSet.Select(result => ((ZString)result[WhsDocketLineSchema.Constants.WE_PackageGroupId]).ToUpper()).ToHashSet();
			});
		}

		BusinessObjectFactory Factory { get; }
		WhsReceive Parent { get; }

		public bool IsPackageGroupAlreadyAssigned(string packingGroupId)
		{
			return PackageGroupIdInfosCache.Value.Contains(packingGroupId.ToUpper());
		}

		Lazy<HashSet<ZString>> PackageGroupIdInfosCache { get; }
	}
}
