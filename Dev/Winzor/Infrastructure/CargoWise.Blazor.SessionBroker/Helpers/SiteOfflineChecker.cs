using System;
using CargoWise.Blazor.SessionBroker.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace CargoWise.Blazor.SessionBroker.Helpers
{
	public interface ISiteOfflineChecker
	{
		bool IsSiteOffline();
		string GetOfflineMessage();
	}

	public class SiteOfflineChecker : ISiteOfflineChecker
	{
		readonly IDatabaseAccessor databaseAccessor;

		readonly IMemoryCache memoryCache;
		public SiteOfflineChecker(IDatabaseAccessor databaseAccessor, IMemoryCache memoryCache)
		{
			this.databaseAccessor = databaseAccessor;
			this.memoryCache = memoryCache;
		}

		public bool IsSiteOffline() => !string.IsNullOrEmpty(Message);

		public string GetOfflineMessage() => Message;

		string Message => memoryCache.GetOrCreate(nameof(SiteOfflineChecker) + "." + nameof(Message),
			(_) => databaseAccessor.ExecuteScalar<string>("SELECT COALESCE(CONVERT(nvarchar(max), SD_BinaryValue), '') FROM dbo.StmData WHERE SD_Name = 'WebVersionSiteOfflineMessage'"),
			new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15) });
	}

	// This is included in .NET 9
	static class MemoryCacheExtensions
	{
		public static TItem GetOrCreate<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, TItem> factory, MemoryCacheEntryOptions createOptions)
		{
			if (!cache.TryGetValue(key, out object result))
			{
				using ICacheEntry entry = cache.CreateEntry(key);

				if (createOptions != null)
				{
					entry.SetOptions(createOptions);
				}

				result = factory(entry);
				entry.Value = result;
			}

			return (TItem)result;
		}
	}
}
