using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Enterprise.Services.Scim.Business;
using Newtonsoft.Json;

namespace Enterprise.Services.Scim.Api.Test
{
#nullable enable

	public class TestIPSafelistHelper : IIPSafelistHelper
	{
		public const string SAFELIST_CACHE_KEY = "IP_SAFELIST";

		public Task<string[]> GetSafelistedIps()
		{
			try
			{
				var cache = MemoryCache.Default;
				var jsonFilePath = Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"net8.0", "test-ip-ranges.json");

				if (!File.Exists(jsonFilePath))
				{
					throw new FileNotFoundException($"IP ranges file not found: {jsonFilePath}");
				}

				var jsonContent = File.ReadAllText(jsonFilePath);
				return Task.FromResult(ProcessJsonContent(jsonContent, cache));
			}
			catch (Exception)
			{
				throw;
			}
		}

		string[] ProcessJsonContent(string jsonContent, MemoryCache cache)
		{
			var azureIpRanges = JsonConvert.DeserializeObject<AzureIpRanges>(jsonContent);

			if (azureIpRanges?.Values == null)
			{
				throw new InvalidOperationException("Could not find IP ranges in JSON");
			}

			var ipList = new List<string>();
			foreach (var item in azureIpRanges.Values)
			{
				if (item.Name == "AzureActiveDirectory" && item.Properties?.AddressPrefixes != null)
				{
					ipList.AddRange(item.Properties.AddressPrefixes);
				}
			}

			if (ipList.Count == 0)
			{
				throw new InvalidOperationException("No IP safelist networks configured");
			}

			var safelistedIps = ipList.ToArray();

			cache.Set(SAFELIST_CACHE_KEY, safelistedIps, new CacheItemPolicy
			{
				AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration
			});

			return safelistedIps;
		}

		class AzureIpRanges
		{
			[JsonProperty("values")]
			public List<AzureService>? Values { get; set; }
		}

		class AzureService
		{
			[JsonProperty("name")]
			public string? Name { get; set; }

			[JsonProperty("properties")]
			public ServiceProperties? Properties { get; set; }
		}

		class ServiceProperties
		{
			[JsonProperty("addressPrefixes")]
			public List<string>? AddressPrefixes { get; set; }
		}
	}
}
