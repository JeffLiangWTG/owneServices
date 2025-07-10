using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common.Cache;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Tools.Enums;

namespace Enterprise.Rating.Business.WiseRates;

public class WiseRatesClientWithCache : WiseRatesClient
{
	public WiseRatesClientWithCache(string apiURL, string accessToken, bool enableDiagnostics = false)
		: base(apiURL, accessToken, enableDiagnostics)
	{
	}

	/// <summary>
	///		Constructor for tests.
	/// </summary>
	public WiseRatesClientWithCache(string apiURL, string accessToken, HttpMessageHandler innerHandler = null)
		: base(apiURL, accessToken, innerHandler)
	{
	}

	static MemoryCache Cache { get; } = MemoryCache.Default;

	public override async Task<RatesSearchResponse> SearchAsync(RatesSearchRequest request, string correlationID = null, CancellationToken cancellationToken = new CancellationToken())
	{
		var retrievedFromCache = true;
		var key = request.GetUniqueKey();

		if (Cache.Get(key) is not RatesSearchResponse response)
		{
			retrievedFromCache = false;

			response = await base.SearchAsync(request, correlationID, cancellationToken).ConfigureAwait(false);

			var shouldCache = response.Providers == null || response.Providers.All(x => x.ConnectionResult == ConnectionResult.Success);
			if (shouldCache)
			{
				var entry = new CacheItem(key);
				entry.Value = response;

				Cache.Add(entry, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(1) });
			}
		}

		var clone = response.DeepClone();
		clone.IsFromCache = retrievedFromCache;

		return clone;
	}

	public override RefChargeCode[] GetChargeCodes(string correlationID = null)
	{
		return GetChargeCodesAsync(correlationID).ConfigureAwait(false).GetAwaiter().GetResult();
	}

	public override Task<RefChargeCode[]> GetChargeCodesAsync(string correlationID = null)
	{
		return Cache.GetOrAdd(
			$"{ServiceURL} ChargeCodes",
			() => base.GetChargeCodesAsync(correlationID),
			new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(24) });
	}

	public override RefServiceLevel[] GetServiceLevels(string correlationID = null)
	{
		return GetServiceLevelsAsync(correlationID).ConfigureAwait(false).GetAwaiter().GetResult();
	}

	public override Task<RefServiceLevel[]> GetServiceLevelsAsync(string correlationID = null)
	{
		return Cache.GetOrAdd(
			$"{ServiceURL} ServiceLevels",
			() => base.GetServiceLevelsAsync(correlationID),
			new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(24) });
	}

	public override RefCommodityGroup[] GetCommodityGroups(string correlationID = null)
	{
		return GetCommodityGroupsAsync(correlationID).ConfigureAwait(false).GetAwaiter().GetResult();
	}

	public override Task<RefCommodityGroup[]> GetCommodityGroupsAsync(string correlationID = null)
	{
		return Cache.GetOrAdd(
			$"{ServiceURL} CommodityGroups",
			() => base.GetCommodityGroupsAsync(correlationID),
			new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(24) });
	}

	public override RatesServiceConfiguration GetConfiguration(string correlationID = null)
	{
		return GetConfigurationAsync(correlationID).ConfigureAwait(false).GetAwaiter().GetResult();
	}

	public override Task<RatesServiceConfiguration> GetConfigurationAsync(string correlationID = null)
	{
		return Cache.GetOrAdd(
			$"{ServiceURL} configurations",
			() => base.GetConfigurationAsync(correlationID),
			new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(24) });
	}
}