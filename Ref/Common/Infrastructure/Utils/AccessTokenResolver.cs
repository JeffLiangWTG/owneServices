using System;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class AccessTokenResolver
	{
		public static string ResolveAccessToken(string authHeader)
		{
			var accessToken = string.Empty;
			if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			{
				accessToken = authHeader.Substring(7);
			}
			return accessToken;
		}
	}
}
