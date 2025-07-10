using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class PendingOrdersCacheManager
	{
		const int MaxPendingOrders = 8;
		const string MoreThanMaxPendingOrdersToShowIndicator = "...";
		const string PendingOrderSeparator = ", ";

		Func<IEnumerable<ZGuid>> ProductPKsProvider { get; set; }

		public static IDisposable UseInventoryPendingOrdersCache(BusinessObjectFactory factory, Func<IEnumerable<ZGuid>> productPKsProvider = null)
		{
			var cacheManager = GetPendingOrderCacheManager(factory);
			SemaphoreManager manager = null;
			return new DisposableAction(
				() =>
				{
					manager = new SemaphoreManager(cacheManager.UseCacheSemaphore);
					cacheManager.ProductPKsProvider = productPKsProvider;
				},
				() =>
				{
					manager?.Dispose();
					cacheManager.ResetCache();
				}
			);
		}

		public static string GetPendingOrders(BusinessObjectFactory factory, ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			var cacheManager = GetPendingOrderCacheManager(factory);
			return cacheManager.CurrentPendingOrders(factory, whsPK, clientPK, productPK);
		}

		static PendingOrdersCacheManager GetPendingOrderCacheManager(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(nameof(PendingOrdersCacheManager), () => new PendingOrdersCacheManager());
		}

		string CurrentPendingOrders(BusinessObjectFactory factory, ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			return UseCacheSemaphore.IsSuspended
				? GetPendingOrdersFromCache(factory, whsPK, clientPK, productPK)
				: GetPendingOrdersFromFactory(factory, whsPK, clientPK, productPK);
		}

		#region GetPendingOrdersFromCache

		string GetPendingOrdersFromCache(BusinessObjectFactory factory, ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			if (!IsProductCached(whsPK, clientPK, productPK))
			{
				PopulateAllProductPendingOrdersCache(factory, whsPK, clientPK, productPK);
			}

			return GetCachedPendingOrders(factory, whsPK, clientPK, productPK);
		}

		bool IsProductCached(ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			var key = GetKey(whsPK, clientPK, productPK);
			return Cache.ContainsKey(key);
		}

		void PopulateAllProductPendingOrdersCache(BusinessObjectFactory factory, ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			IEnumerable<ZGuid> productPKs = new[] { productPK };
			if (ProductPKsProvider != null)
			{
				productPKs = productPKs.Union(ProductPKsProvider());
			}

			var pendingOrders = GetAllProductPendingOrdersFromFactory(factory, whsPK, clientPK, productPKs);
			foreach (var prodPK in productPKs)
			{
				var key = GetKey(whsPK, clientPK, prodPK);
				var pendingOrdersForProduct = pendingOrders.Where(pendingOrder => (ZGuid)pendingOrder[WhsDocketLineSchema.WE_OP] == prodPK);
				Cache.Add(key, GetPendingOrdersString(pendingOrdersForProduct));
			}
		}

		string GetCachedPendingOrders(BusinessObjectFactory factory, ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			var key = GetKey(whsPK, clientPK, productPK);
			if (!Cache.TryGetValue(key, out var pendingOrders))
			{
				pendingOrders = GetPendingOrdersFromFactory(factory, whsPK, clientPK, productPK);
				Cache.Add(key, pendingOrders);
			}

			return pendingOrders;
		}

		static string GetKey(ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			return string.Format(Culture.Invariant, $"{whsPK}#{clientPK}#{productPK}"); // Dictionary key
		}

		#endregion

		#region GetPendingOrdersFromFactory

		string GetPendingOrdersFromFactory(BusinessObjectFactory factory, ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			var pendingOrders = LoadPendingOrders(factory, whsPK, clientPK, productPK);
			var pendingOrderStringBuilder = new ZStringBuilder();

			var docketLineQuery = new ZQuery { AllowTableValuedParameters = true };
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, pendingOrders.Select(o => o.PK));
			factory.AddFetchHint(WhsDocketLineSchema.Instance, docketLineQuery);

			var pickLineQuery = new ZQuery { AllowTableValuedParameters = true };
			pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, pendingOrders.SelectMany(o => o.Lines).Select(l => l.PK));
			factory.AddFetchHint(WhsPickLineSchema.Instance, pickLineQuery);

			var pendingOrdersCount = 0;
			foreach (var order in pendingOrders)
			{
				if (pendingOrdersCount == MaxPendingOrders)
				{
					pendingOrderStringBuilder.Append("...");
					break;
				}

				foreach (WhsOrderLine orderLine in order.Lines)
				{
					if (orderLine.WE_OP == productPK)
					{
						if (orderLine.WE_CrossDockQuantity < orderLine.WE_TransactionQuantity)
						{
							pendingOrderStringBuilder.Append(order.WD_ExternalReference);
							pendingOrdersCount++;
							break;
						}
					}
				}
			}

			return pendingOrderStringBuilder.ToStringWithDelimiterBetweenAppends(", ");
		}

		static WhsOrder[] LoadPendingOrders(BusinessObjectFactory factory, ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			return factory.Load<WhsOrder>(GetPendingOrdersQuery(whsPK, clientPK, productPK));
		}

		static ZQuery GetPendingOrdersQuery(ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			var orderQuery = new ZDBOnlyQuery(typeof(WhsOrder));
			orderQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, whsPK);
			orderQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, clientPK);
			orderQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			orderQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, DocketStatus.Codes.Entered);
			orderQuery.OrderBy = WhsDocketSchema.WD_ExternalReference.Name;

			var orderLineFilter = new ZDBOnlySubQuery(typeof(WhsOrderLine), WhsDocketLineSchema.WE_WD);
			orderLineFilter.AddToFilter(WhsDocketLineSchema.WE_OP, productPK);
			orderLineFilter.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, string.Empty);
			orderQuery.AddSubQuery(orderLineFilter, JoinCondition.And);

			return orderQuery;
		}

		#endregion

		#region GetAllProductPendingOrdersFromFactory

		IEnumerable<DynamicBusinessObject> GetAllProductPendingOrdersFromFactory(BusinessObjectFactory factory, ZGuid whsPK, ZGuid clientPK, IEnumerable<ZGuid> productPKs)
		{
			var collection = new DynamicBusinessObjectCollection(factory);
			var sql = $@"
;with RankedOrdersAndOrderLinesPerProduct as
(
	SELECT
		ROW_NUMBER() OVER (PARTITION BY WE_OP ORDER BY WD_ExternalReference) as RowNumber,
		WD_ExternalReference,
		WE_OP
	FROM
		dbo.WhsDocket
		JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
		CROSS APPLY
		(
			SELECT
				SUM(WZ_Units) AS ReservedQuantity
			FROM
				dbo.WhsPickLine
			WHERE
				WZ_WE_TransactionLine = WE_PK
				AND WZ_PickedDateTime is null
				AND WZ_OriginalReservedQty > 0
		) AS ReservedPickLines
	WHERE
		WD_DocketType = '{DocketType.Codes.Order}'
		AND WD_DocketStatus = '{DocketStatus.Codes.Entered}'
		AND WD_WW_Whs = @WarehousePK
		AND WD_OH_Client = @ClientPK
		AND WE_TransactionQuantity > isnull(ReservedQuantity,0)
		AND WE_OP IN (SELECT Value FROM @ProductPKs)
		AND WE_DocketLineStatus = ''
	GROUP BY
		WD_ExternalReference,
		WE_OP
)

SELECT
	WD_ExternalReference,
	WE_OP
FROM
	RankedOrdersAndOrderLinesPerProduct
WHERE
	RowNumber <= @MaxPendingOrdersToShow
";
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WarehousePK", whsPK, WhsDocketSchema.WD_WW_Whs);
			sqlParams.Add("@ClientPK", clientPK, WhsDocketSchema.WD_OH_Client);
			sqlParams.Add(ZSqlParameter.New("@ProductPKs", productPKs.ToArray(), WhsDocketLineSchema.WE_OP, isTableValued: true));
			sqlParams.Add("@MaxPendingOrdersToShow", MaxPendingOrders + 1, WhsDocketSchema.WD_PackagesSent);
			collection.Load(sql, sqlParams);

			return collection;
		}

		string GetPendingOrdersString(IEnumerable<DynamicBusinessObject> pendingOrders)
		{
			var pendingOrderStringBuilder = new ZStringBuilder();
			var pendingOrdersCount = 0;
			var externalReferences = pendingOrders.Select(p => (ZString)p[WhsDocketSchema.WD_ExternalReference]).OrderBy(p => p);
			foreach (var externalReference in externalReferences)
			{
				if (pendingOrdersCount == MaxPendingOrders)
				{
					pendingOrderStringBuilder.Append(MoreThanMaxPendingOrdersToShowIndicator);
					break;
				}

				pendingOrderStringBuilder.Append(externalReference);
				pendingOrdersCount++;
			}

			return pendingOrderStringBuilder.ToStringWithDelimiterBetweenAppends(PendingOrderSeparator);
		}

		#endregion

		#region Implementation

		Semaphore UseCacheSemaphore => useCacheSemaphore ?? (useCacheSemaphore = new Semaphore());
		Semaphore useCacheSemaphore;

		void ResetCache()
		{
			Cache.Clear();
			ProductPKsProvider = null;
		}

		Dictionary<string, string> Cache => cache ?? (cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		Dictionary<string, string> cache;

		#endregion
	}
}
