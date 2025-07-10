using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBondedWarehouseAttributeCache
	{
		public WhsBondedWarehouseAttributeCache(BusinessObjectFactory factory, IEnumerable<WhsDocketLine> docketLines)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(docketLines, nameof(docketLines));

			EntryKeyCache = new Lazy<HashSet<ZString>>(() =>
			{
				var entryKeyCache = new Dictionary<ZGuid, ZString>();
				var docketLinePKsToInclude = docketLines
					.Where(docketLine => docketLine.IsInDatabase)
					.Select(docketLine => docketLine.PK)
					.ToArray();

				if (docketLinePKsToInclude.Length > 0)
				{
					var resultSet = new DynamicBusinessObjectCollection(Factory);
					resultSet.Load(@"
SELECT
	WB_ParentId,
	WB_EntryKey
FROM
	dbo.WhsBondedWarehouseAttribute
WHERE
	WB_ParentID IN (SELECT Value FROM @DocketLineParentPKs)
", new[] { ZSqlParameter.New("@DocketLineParentPKs", docketLinePKsToInclude, WhsBondedWarehouseAttributeSchema.WB_ParentID, isTableValued: true) });

					entryKeyCache = resultSet.ToDictionary(r => (ZGuid)r[WhsBondedWarehouseAttributeSchema.Constants.WB_ParentID], r => ((ZString)r[WhsBondedWarehouseAttributeSchema.Constants.WB_EntryKey]).ToUpper());
				}

				docketLines
					.Where(docketLine => !docketLine.IsInDatabase || docketLine.HasChanges)
					.ForEach(docketLine => entryKeyCache[docketLine.PK] = docketLine.CustomsData.WB_EntryKey.ToUpper());

				return entryKeyCache.Values.Where(entryKey => !entryKey.IsEmpty).ToHashSet();
			});
		}

		BusinessObjectFactory Factory { get; }

		public bool IsMismatchedEntryKey(WhsBondedWarehouseAttribute bondedWarehouseAttribute)
		{
			return EntryKeyCache.Value.Contains(bondedWarehouseAttribute.WB_EntryKey) && EntryKeyCache.Value.Count > 1;
		}

		Lazy<HashSet<ZString>> EntryKeyCache { get; }
	}
}
