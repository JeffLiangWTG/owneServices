using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class PalletIDPutawayTransferCacheManager
	{
		public static IDisposable UseHasPutawayTransferCache(BusinessObjectFactory factory)
		{
			var cache = GetBoolCache(factory);
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

		public static bool HasPutawayTransferForPallet(WhsReceive receive, ZString palletID)
		{
			var cache = GetBoolCache(receive.Factory);
			return cache.GetHasPutawayTransferForPalletID(receive, palletID);
		}

		static PalletIDPutawayTransferCacheManager GetBoolCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(nameof(PalletIDPutawayTransferCacheManager), () => new PalletIDPutawayTransferCacheManager(), CacheStalenessPolicy.StaleOnFactorySave);
		}

		bool GetHasPutawayTransferForPalletID(WhsReceive receive, ZString palletID)
		{
			return UseCacheSemaphore.IsSuspended
				? GetCachedHasPutawayTransfer(receive, palletID)
				: GetHasPutawayTransferFromReceive(receive, palletID);
		}

		bool GetCachedHasPutawayTransfer(WhsReceive receive, ZString palletID)
		{
			var key = receive.PK.ToString() + "#" + palletID;
			if (!Cache.TryGetValue(key, out var hasPutawayTransfer))
			{
				hasPutawayTransfer = GetHasPutawayTransferFromReceive(receive, palletID);
				Cache.Add(key, hasPutawayTransfer);
			}

			return hasPutawayTransfer;
		}

		bool GetHasPutawayTransferFromReceive(WhsReceive receive, ZString palletID)
		{
			return receive.Lines.Where(l => l.WE_PalletID.EqualsIgnoringCase(palletID)).Cast<WhsReceiveLine>().Any(l => l.HasPutawayTransfer);
		}

		#region Implementation

		void ClearCache() => Cache.Clear();

		Semaphore UseCacheSemaphore
		{
			get { return useCacheSemaphore ?? (useCacheSemaphore = new Semaphore()); }
		}
		Semaphore useCacheSemaphore;

		Dictionary<string, bool> Cache
		{
			get { return cache ?? (cache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)); }
		}
		Dictionary<string, bool> cache;

		#endregion
	}
}
