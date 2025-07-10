using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.ServiceTask
{
	public interface ISynchronisationOrgQueue
	{
		int GetOrgsToProcessTotalCount(bool onlyOrgsRequiringFullSync);
		Guid[] GetOrgPksToSync(bool onlyOrgsRequiringFullSync, int batchSize);
	}

	public class SalesTradeLanesSynchronisationOrgQueue : ISynchronisationOrgQueue
	{
		public SalesTradeLanesSynchronisationOrgQueue(CachedTradeLinesSummaryProvider summaryProvider)
		{
			Argument.NotNull(summaryProvider, "summaryProvider");

			Connection = summaryProvider.Connection;
			CacheFromMonthString = summaryProvider.CacheFrom.Year + ((ZString)summaryProvider.CacheFrom.Month.ToString(CultureInfo.InvariantCulture)).PadLeft(2, '0');
			CacheFrom = summaryProvider.CacheFrom;
			CacheTo = summaryProvider.CacheTo;
			OrgCaches = CreateOrgCache();
		}

		readonly DbConnection Connection;
		readonly LinkedList<OrgCache> OrgCaches;
		readonly string CacheFromMonthString;
		readonly ZDate CacheFrom;
		readonly ZDate CacheTo;

		public int GetOrgsToProcessTotalCount(bool onlyOrgsRequiringFullSync) => onlyOrgsRequiringFullSync ? OrgCaches.Count(x => !x.HasSyncAll) : OrgCaches.Count;

		public Guid[] GetOrgPksToSync(bool onlyOrgsRequiringFullSync, int batchSize)
		{
			var result = new List<Guid>();

			if (OrgCaches.Any())
			{
				var curNode = OrgCaches.First;
				var nextNode = curNode.Next;

				while (curNode != null && result.Count < batchSize)
				{
					nextNode = curNode.Next;
					if (!onlyOrgsRequiringFullSync || !curNode.Value.HasSyncAll)
					{
						result.Add(curNode.Value.OH_PK);
						OrgCaches.Remove(curNode);
					}
					curNode = nextNode;
				}
			}

			return result.ToArray();
		}

		#region SuppressResourceStringsCheckRegion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		string CreateOrgCacheQuery
		{
			get
			{
				return
$@"SELECT 
	OH_PK,
	MAX(SL_EventTime) LastSyncEventTime,
	MAX(CASE WHEN SL_Reference LIKE '{TradeLinesSynchroniser.SynchronisedAllReference} {CacheFromMonthString}%' AND SL_EventTime > @SyncDateTimeThreshold THEN 1 ELSE 0 END) HasSyncAll" +
$@"
FROM 
	(
		SELECT DISTINCT MainOrg AS OrgPK FROM {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} WHERE MainOrg IS NOT NULL
		UNION
		SELECT DISTINCT OW_OH_Supplier AS OrgPk FROM dbo.OrgSales o
		INNER JOIN dbo.OrgTradeDetail ON PA_OW = OW_PK
		INNER JOIN dbo.OrgTradePeriod ON PAS_PA = PA_PK
		WHERE PAS_Period >= @PeriodFrom AND PAS_Period < @PeriodTo AND OW_IsTraded = 1 AND OW_OH_Supplier IS NOT NULL
		UNION
		SELECT DISTINCT OW_OH_Buyer AS OrgPk FROM dbo.OrgSales o
		INNER JOIN dbo.OrgTradeDetail ON PA_OW = OW_PK
		INNER JOIN dbo.OrgTradePeriod ON PAS_PA = PA_PK
		WHERE PAS_Period >= @PeriodFrom AND PAS_Period < @PeriodTo AND OW_IsTraded = 1 AND OW_OH_Buyer IS NOT NULL
		UNION
		SELECT DISTINCT OW_OH_Primary AS OrgPk FROM dbo.OrgSales o
		INNER JOIN dbo.OrgTradeDetail ON PA_OW = OW_PK
		INNER JOIN dbo.OrgTradePeriod ON PAS_PA = PA_PK
		WHERE PAS_Period >= @PeriodFrom AND PAS_Period < @PeriodTo AND OW_IsTraded = 1 AND OW_OH_Primary IS NOT NULL
		UNION
		SELECT DISTINCT PAS_OH_Client AS OrgPk FROM dbo.OrgSales o
		INNER JOIN dbo.OrgTradeDetail ON PA_OW = OW_PK
		INNER JOIN dbo.OrgTradePeriod ON PAS_PA = PA_PK
		WHERE PAS_Period >= @PeriodFrom AND PAS_Period < @PeriodTo AND OW_IsTraded = 1
	) DistinctSalesOrgPKs
	JOIN dbo.OrgHeader
		ON OrgPK = OH_PK
	LEFT JOIN dbo.StmALog
		ON SL_Parent = OH_PK AND SL_SE_NKEvent = 'TLS'
WHERE 
	OH_IsActive = 1
GROUP BY
	OH_PK
ORDER BY LastSyncEventTime, OH_PK";
			}
		}

		ZSqlParameterCollection CreateOrgCacheQueryParameters
		{
			get
			{
				var queryParameters = new ZSqlParameterCollection
				{
					ZSqlParameter.New("@SyncDateTimeThreshold", OrganisationRegistry.Instance.FullTradeLanesSyncDateTimeThreshold.Value, StmALogSchema.SL_EventTime),
					ZSqlParameter.New("@PeriodFrom", CacheFrom, OrgTradePeriodSchema.PAS_Period),
					ZSqlParameter.New("@PeriodTo", CacheTo, OrgTradePeriodSchema.PAS_Period),
				};
				return queryParameters;
			}
		}

		LinkedList<OrgCache> CreateOrgCache()
		{
			var collection = new DynamicBusinessObjectCollection<OrgCache>(new BusinessObjectFactory(Connection));
			collection.Load(CreateOrgCacheQuery, CreateOrgCacheQueryParameters);
			return new LinkedList<OrgCache>(collection);
		}

		internal class OrgCache : DynamicBusinessObject, IObsoleteValidation
		{
			public static class Schema
			{
				public const string OH_PK = "OH_PK";
				public const string LastSyncEventTime = "LastSyncEventTime";
				public const string HasSyncAll = "HasSyncAll";
			}

			public OrgCache(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				var pk = new ZGuid(row[Schema.OH_PK]);
				OH_PK = pk.IsValid ? pk.ToGuid() : Guid.Empty;

				LastSyncEventTime = new ZDateTime(row[Schema.LastSyncEventTime]);
				HasSyncAll = new ZBool(row[Schema.HasSyncAll]);
			}

			public Guid OH_PK { get; private set; }
			public ZDateTime LastSyncEventTime { get; private set; }
			public ZBool HasSyncAll { get; private set; }
		}

		#endregion
	}

	public class RetrySynchronisationOrgQueue : ISynchronisationOrgQueue
	{
		public RetrySynchronisationOrgQueue(Queue<Guid> fullSyncOrgPks, Queue<Guid> partialSyncOrgPks)
		{
			this.fullSyncOrgPks = fullSyncOrgPks;
			this.partialSyncOrgPks = partialSyncOrgPks;
		}

		readonly Queue<Guid> fullSyncOrgPks;
		readonly Queue<Guid> partialSyncOrgPks;

		public Guid[] GetOrgPksToSync(bool onlyOrgsRequiringFullSync, int batchSize)
		{
			var queue = onlyOrgsRequiringFullSync ? fullSyncOrgPks : partialSyncOrgPks;
			return DequeueBatch(queue, batchSize).ToArray();
		}

		static IEnumerable<T> DequeueBatch<T>(Queue<T> queue, int batchSize)
		{
			for (var i = 0; i < batchSize; i++)
			{
				if (queue.Count == 0)
				{
					break;
				}

				yield return queue.Dequeue();
			}
		}

		public int GetOrgsToProcessTotalCount(bool onlyOrgsRequiringFullSync)
		{
			var queue = onlyOrgsRequiringFullSync ? fullSyncOrgPks : partialSyncOrgPks;
			return queue.Count;
		}
	}
}
