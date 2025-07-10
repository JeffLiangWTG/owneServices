using System;
using System.Linq;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public static class IdpEndpointUriProvider
	{
		static string IdpUserSyncBaseUrl => SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.Value.TrimEnd('/');

		public static Uri GetIdpUserSyncBaseUri()
		{
			return GetIdpUserSyncUri();
		}

		public static Uri GetBulkCreateUsersUri()
		{
			return GetIdpUserSyncUri("bulkCreate");
		}

		static Uri GetIdpUserSyncUri(params string[] pathSegments)
		{
			if (string.IsNullOrWhiteSpace(IdpUserSyncBaseUrl))
			{
				throw new IdpConfigException(
					Res.GetString(
						"b92d2abf-c4f3-48c9-8036-5de7d4ffcf09",
						"Identity Provider user synchronization endpoint hasn't been configured in the registry. Please configure it here: {0}",
						SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.GetLocation()));
			}

			var url = string.Join("/", pathSegments.Prepend(IdpUserSyncBaseUrl));
			if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
			{
				return uri;
			}
			throw new IdpConfigException(
				Res.GetString(
					"1ca3a1b1-8d39-4aa2-998e-29b63f17b133",
					"Identity Provider user synchronization endpoint config is not formatted correctly in {0}",
					SystemDataRegistry.Instance.IdpUserSynchronisationEndpoint.GetLocation()));
		}
	}
}
