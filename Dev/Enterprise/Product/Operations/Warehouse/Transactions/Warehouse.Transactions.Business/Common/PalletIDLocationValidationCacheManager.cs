using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class PalletIDLocationValidationCacheManager
	{
		public static IDisposable UseLocationCache(BusinessObjectFactory factory)
		{
			var cache = GetLocationCache(factory);
			SemaphoreManager manager = null;
			return new DisposableAction(
				() => manager = new SemaphoreManager(cache.UseCacheSemaphore),
				() =>
				{
					manager?.Dispose();
					cache.ClearCache();
				}
			);
		}

		public static IEnumerable<WhsLocation> GetUniqueLocationsWithStockForPalletID(BusinessObjectFactory factory, ZGuid whsPK, ZString palletID)
		{
			var cache = GetLocationCache(factory);
			return cache.GetOtherLocationsForPalletID(factory, whsPK, palletID);
		}

		static PalletIDLocationValidationCacheManager GetLocationCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(nameof(PalletIDLocationValidationCacheManager), () => new PalletIDLocationValidationCacheManager(), CacheStalenessPolicy.StaleOnFactorySave);
		}

		IEnumerable<WhsLocation> GetOtherLocationsForPalletID(BusinessObjectFactory factory, ZGuid whsPK, string palletID)
		{
			return UseCacheSemaphore.IsSuspended
				? GetCachedLocations(factory, whsPK, palletID)
				: GetLocationsWithStockIncludingNotYetFinalised(factory, whsPK, palletID);
		}

		IEnumerable<WhsLocation> GetCachedLocations(BusinessObjectFactory factory, ZGuid whsPK, string palletID)
		{
			var key = whsPK.ToString() + "#" + palletID;
			if (!Cache.TryGetValue(key, out var locations))
			{
				locations = GetLocationsWithStockIncludingNotYetFinalised(factory, whsPK, palletID);
				Cache.Add(key, locations);
			}

			return locations;
		}

		#region GetLocationsWithStockIncludingNotYetFinalised

		IEnumerable<WhsLocation> GetLocationsWithStockIncludingNotYetFinalised(BusinessObjectFactory factory, ZGuid warehousePK, string palletID)
		{
			var query = WhsValidationHelper.GetLocationsWithStockIncludingNotYetFinalisedQuery(palletID);
			var inventory = factory.Load<WhsDocketLine>(query);
			return GetLocationWithStockIncludingNotYetFinalised(inventory, warehousePK);
		}

		IEnumerable<WhsLocation> GetLocationWithStockIncludingNotYetFinalised(IEnumerable<WhsDocketLine> allInventory, ZGuid warehousePK)
		{
			return IEnumerableExtensions.DistinctBy(allInventory, i => i.WE_WL).Select(i => i.Location).WhereNotNull().Where(l => l.WLV_WW_Whs == warehousePK);
		}

		#endregion

		#region Implementation

		void ClearCache() => Cache.Clear();

		Semaphore UseCacheSemaphore
		{
			get { return useCacheSemaphore ?? (useCacheSemaphore = new Semaphore()); }
		}
		Semaphore useCacheSemaphore;

		Dictionary<string, IEnumerable<WhsLocation>> Cache
		{
			get { return cache ?? (cache = new Dictionary<string, IEnumerable<WhsLocation>>(StringComparer.OrdinalIgnoreCase)); }
		}
		Dictionary<string, IEnumerable<WhsLocation>> cache;

		#endregion
	}
}
