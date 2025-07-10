using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Tracking
{
	public static class TrackingUrlBuilder
	{
		public static string BuildUrl(string rootUrl, Guid contactPK, TrackingConstants.BusinessContext businessContext, Guid businessContextPK, params Guid[] businessContextAdditionalRefs)
		{
			Argument.NotNull(rootUrl, nameof(rootUrl));
			return BuildUrl(rootUrl, contactPK, businessContext, businessContextPK, false, businessContextAdditionalRefs);
		}

		public static string BuildNeoUrl(string rootUrl, string alias, Guid businessContextPK)
		{
			Argument.NotNull(rootUrl, nameof(rootUrl));

			return BuildNeoUrlCore(rootUrl, alias, "entityPK", businessContextPK.ToString());
		}

		public static string BuildNeoUrl(string rootUrl, string alias, string businessContextNK)
		{
			Argument.NotNull(rootUrl, nameof(rootUrl));

			return BuildNeoUrlCore(rootUrl, alias, "entityNK", businessContextNK);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Url")]
		public static string BuildNeoUrlCore(string rootUrl, string alias, string key, string value)
		{
			if (!string.IsNullOrEmpty(rootUrl))
			{
				try
				{
					var uriBuilder = new UriBuilder(rootUrl);
					uriBuilder.Path = string.Format("{0}/goto/{1}", uriBuilder.Path.TrimEnd('/'), alias);
					uriBuilder.Query = string.Format("{0}={1}", key, value);
					return uriBuilder.Uri.AbsoluteUri;
				}
				catch (UriFormatException) { }
			}

			return string.Empty;
		}

		public static string BuildNeoGuestTrackingUrl(string rootUrl, Guid businessContextPK)
		{
			Argument.NotNull(rootUrl, nameof(rootUrl));

			return BuildNeoGuestTrackingUrlCore(rootUrl, "trackingKey", businessContextPK.ToString());
		}

		public static string BuildNeoGuestTrackingUrl(string rootUrl, string businessContextNK)
		{
			Argument.NotNull(rootUrl, nameof(rootUrl));

			return BuildNeoGuestTrackingUrlCore(rootUrl, "trackingNumber", businessContextNK);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Url")]
		static string BuildNeoGuestTrackingUrlCore(string rootUrl, string key, string value)
		{
			if (!string.IsNullOrEmpty(rootUrl))
			{
				try
				{
					var uriBuilder = new UriBuilder(rootUrl);
					uriBuilder.Path = string.Format("{0}/NEO/Desktop", uriBuilder.Path.TrimEnd('/'));
					uriBuilder.Fragment = string.Format("/tracker?{0}={1}", key, value);
					return uriBuilder.Uri.AbsoluteUri;
				}
				catch (UriFormatException) { }
			}

			return string.Empty;
		}

		public static string BuildUrl(string rootUrl, Guid contactPK, TrackingConstants.BusinessContext businessContext, Guid businessContextPK, bool requireLogin, params Guid[] businessContextAdditionalRefs)
		{
			Argument.NotNull(rootUrl, nameof(rootUrl));
			return BuildUrlCore(rootUrl, contactPK, GenerateSecureQueryString(contactPK, businessContext, businessContextPK, requireLogin, businessContextAdditionalRefs));
		}

		public static string BuildUrl(string rootUrl, Guid contactPK, TrackingConstants.BusinessContext businessContext, String businessContextNK, bool requireLogin = false)
		{
			Argument.NotNull(rootUrl, nameof(rootUrl));
			return BuildUrlCore(rootUrl, contactPK, GenerateSecureQueryString(contactPK, businessContext, businessContextNK, requireLogin));
		}

		static string BuildUrlCore(string rootUrl, Guid contactPk, SecureQueryString queryString)
		{
			Argument.NotNull(rootUrl, nameof(rootUrl));	// Suggested By ReviewBot
			Argument.NotNull(queryString, nameof(queryString)); // Suggested By ReviewBot
			if (!string.IsNullOrEmpty(rootUrl))
			{
				try
				{
					var uriBuilder = new UriBuilder(rootUrl);
					uriBuilder.Path = string.Format("{0}/{1}", uriBuilder.Path.TrimEnd('/'), TrackingConstants.RelativePath.AutoLoginRequestHandler);
					uriBuilder.Query = string.Format("{0}={1}", TrackingConstants.AutoLogin.SecureQueryStringDataKey, WebUtility.UrlEncode(queryString.ToString()));
					return uriBuilder.Uri.AbsoluteUri;
				}
				catch (UriFormatException) { }
			}

			return string.Empty;
		}

		static SecureQueryString GenerateSecureQueryString(Guid contactPK, TrackingConstants.BusinessContext businessContext, Guid businessContextPK, bool requireLogin, params Guid[] businessContextAdditionalRefs)
		{
			var queryString = GenerateSecureQueryString(contactPK, businessContext, TrackingConstants.AutoLogin.BusinessContextPKKey, businessContextPK.ToString(), requireLogin);
			if (businessContextAdditionalRefs != null && businessContextAdditionalRefs.Length > 0)
			{
				var refs = new StringBuilder();
				foreach (var reference in businessContextAdditionalRefs)
				{
					refs.Append(reference.ToString() + ",");
				}

				queryString.Add(TrackingConstants.AutoLogin.BusinessContextAdditionalRefsKey, refs.ToString().TrimEnd(','));
			}
			return queryString;
		}

		static SecureQueryString GenerateSecureQueryString(Guid contactPK, TrackingConstants.BusinessContext businessContext, string businessContextNK, bool requireLogin)
		{
			return GenerateSecureQueryString(contactPK, businessContext, TrackingConstants.AutoLogin.BusinessContextNKKey, businessContextNK, requireLogin);
		}

		static SecureQueryString GenerateSecureQueryString(Guid contactPK, TrackingConstants.BusinessContext businessContext, string businessContextRefName, string businessContextRefValue, bool requireLogin)
		{
			var queryString = new SecureQueryString();
			queryString.Add(TrackingConstants.AutoLogin.ContactPKKey, contactPK.ToString());
			if (requireLogin)
			{
				queryString.Add(TrackingConstants.AutoLogin.RequireLoginKey, true.ToString());
			}
			queryString.Add(TrackingConstants.AutoLogin.BusinessContextKey, businessContext.ToString());
			if (businessContextRefName != null && businessContextRefValue != null)
			{
				queryString.Add(businessContextRefName, businessContextRefValue);
			}

			return queryString;
		}
	}
}
