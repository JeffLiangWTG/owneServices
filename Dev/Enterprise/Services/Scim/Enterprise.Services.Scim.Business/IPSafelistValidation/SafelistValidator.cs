using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Enterprise.Services.Scim.Business;

namespace Enterprise.Services.Scim.Api.Helpers
{
	public class SafelistValidator : ISafelistValidator
	{
		readonly IIPSafelistCacheStore Cache;

		public SafelistValidator(IIPSafelistCacheStore cache)
		{
			Cache = cache ?? throw new ArgumentNullException(nameof(cache));
		}

		public bool ValidateIp(string ip, string[] safelistSkippedIps)
		{
			if (string.IsNullOrEmpty(ip))
			{
				throw new ArgumentNullException(nameof(ip));
			}

			if (safelistSkippedIps.Contains(ip))
			{
				return true;
			}

			if (IPAddress.TryParse(ip, out var parsedIp))
			{
				// Check if the IP address is IPv6 and not the loopback address (::1)
				if (parsedIp.AddressFamily == AddressFamily.InterNetworkV6 && !IPAddress.IsLoopback(parsedIp))
				{
					return false;
				}
			}
			else
			{
				throw new FormatException("IP address is in wrong format");
			}

			var safelistedIps = Cache.GetSafelistedIpsFromCache();
			if (safelistedIps == null || safelistedIps.Count == 0)
			{
				throw new InvalidOperationException("No IP safelist networks configured");
			}

			return safelistedIps.Any(safelistedIp => safelistedIp.Contains(parsedIp));
		}
	}

	public interface ISafelistValidator
	{
		public bool ValidateIp(string ip, string[] safelistSkippedIps);
	}
}
