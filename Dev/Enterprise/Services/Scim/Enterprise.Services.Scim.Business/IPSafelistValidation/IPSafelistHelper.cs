using System;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Configuration;
#endif
using System.Net.Http;
#if NETFRAMEWORK
using System.Runtime.Caching;
#endif
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
#if NETCOREAPP
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
#endif

namespace Enterprise.Services.Scim.Business
{
	public class IPSafelistHelper : IIPSafelistHelper
	{
		public const string SAFELIST_CACHE_KEY = "IP_SAFELIST";
		const int MAX_RETRIES = 3;
		const int TIMEOUT = 30;
		const string WHITELIST_URL = "https://www.microsoft.com/en-us/download/confirmation.aspx?id=56519";
#pragma warning disable CW1021 // Static Fields Are Thread Static Rule
		static readonly Uri[] ALLOWED_DOMAINS = {
			new Uri("https://download.microsoft.com/"),
			new Uri("https://www.microsoft.com/")
		};
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule
		readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public async Task<string[]> GetSafelistedIps()
		{
#if NETFRAMEWORK
			var cache = MemoryCache.Default;
			if (cache.Contains(SAFELIST_CACHE_KEY))
			{
				return (string[])cache.Get(SAFELIST_CACHE_KEY);
			}
#else
			var cache = new MemoryCache(new MemoryCacheOptions());
			if (cache.TryGetValue(SAFELIST_CACHE_KEY, out var cacheValue))
			{
				return (string[])cacheValue;
			}
#endif
			try
			{
				var jsonContent = await DownloadJsonContentWithRetry();
				return ProcessJsonContent(jsonContent, cache);
			}
			catch (Exception ex)
			{
				logger.Error("IPSafelistHelper.GetSafelistedIps", ex.Message);
				throw;
			}
		}

		async Task<string> DownloadJsonContentWithRetry()
		{
			var attempts = 0;
			Exception lastException = null;

			while (attempts < MAX_RETRIES)
			{
				try
				{
					using (var client = new HttpClient())
					{
						client.Timeout = TimeSpan.FromSeconds(TIMEOUT);

#if NETFRAMEWORK
						var downloadUrl = ConfigurationManager.AppSettings["WhitelistURL"] ?? WHITELIST_URL;
#else
						var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
						var downloadUrl = configuration["WhitelistURL"] ?? WHITELIST_URL;
#endif
						ValidateUrl(downloadUrl);

						var downloadPage = await client.GetStringAsync(downloadUrl);

						var jsonUrl = ExtractJsonUrlSafely(downloadPage);

						ValidateUrl(jsonUrl);

						return await client.GetStringAsync(jsonUrl);
					}
				}
				catch (SecurityException ex)
				{
					logger.Error($"Potentially malicious html injected: {ex}");
					throw;
				}
				catch (Exception ex)
				{
					lastException = ex;
					attempts++;

					if (attempts < MAX_RETRIES)
					{
						await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts)));
					}
					logger.Error(ex);
				}
			}

			throw new InvalidOperationException($"Failed to download IP safelist after {MAX_RETRIES} attempts", lastException);
		}

		void ValidateUrl(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				logger.Error($"URL cannot be null or empty URL: {url}");
				throw new ArgumentException("URL cannot be null or empty");
			}

			if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
			{
				logger.Error($"Invalid URL format: {url}");
				throw new ArgumentException($"Invalid URL format: {url}");
			}

			var isAllowedDomain = false;
			foreach (var allowedDomain in ALLOWED_DOMAINS)
			{
				if (uri.Host.Equals(allowedDomain.Host, StringComparison.OrdinalIgnoreCase))
				{
					isAllowedDomain = true;
					break;
				}
			}

			if (!isAllowedDomain)
			{
				logger.Error($"URL domain not allowed: {uri.Host}");
				throw new SecurityException($"URL domain not allowed: {uri.Host}");
			}
		}

		public string ExtractJsonUrlSafely(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				logger.Error($"HTML content cannot be null or empty {html}");
				throw new ArgumentException("HTML content cannot be null or empty");
			}

			var regex = new Regex(@"href\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
			var matches = regex.Matches(html);

			foreach (Match match in matches)
			{
				if (match.Groups[1].Success)
				{
					var url = match.Groups[1].Value;
					try
					{
						var uri = new Uri(url);

						if (string.Equals(uri.Scheme, $"https", StringComparison.OrdinalIgnoreCase) &&
							string.Equals(uri.Host, "download.microsoft.com", StringComparison.Ordinal) &&
							uri.AbsolutePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
						{
							return url;
						}
					}
					catch (UriFormatException ex)
					{
						logger.Error($"URI Formatting Error: {ex}");
						continue;
					}
				}
			}

			throw new InvalidOperationException("Could not find valid Microsoft JSON URL in download page");
		}

		string[] ProcessJsonContent(string jsonContent, MemoryCache cache)
		{
			if (string.IsNullOrWhiteSpace(jsonContent))
			{
				throw new ArgumentException("JSON content cannot be null or empty");
			}

			var azureIpRanges = JsonConvert.DeserializeObject<AzureIpRanges>(jsonContent);
			if (azureIpRanges?.Values == null)
			{
				logger.Error($"Could not find IP ranges in JSON, JSON: {jsonContent}");
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
				logger.Error($"No IP safelist networks configured");
				throw new InvalidOperationException("No IP safelist networks configured");
			}

			var safelistedIps = ipList.ToArray();

#if NETFRAMEWORK
			var cachePolicy = new CacheItemPolicy
			{
				AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration
			};
#else
			var cachePolicy = new MemoryCacheEntryOptions
			{
				AbsoluteExpiration = DateTimeOffset.MaxValue
			};
#endif

			cache.Set(SAFELIST_CACHE_KEY, safelistedIps, cachePolicy);

			return safelistedIps;
		}

		class AzureIpRanges
		{
			[JsonProperty("values")]
			public List<AzureService> Values { get; set; }
		}

		class AzureService
		{
			[JsonProperty("name")]
			public string Name { get; set; }
			[JsonProperty("properties")]
			public ServiceProperties Properties { get; set; }
		}

		class ServiceProperties
		{
			[JsonProperty("addressPrefixes")]
			public List<string> AddressPrefixes { get; set; }
		}
	}

	public interface IIPSafelistHelper
	{
		Task<string[]> GetSafelistedIps();
	}
}
