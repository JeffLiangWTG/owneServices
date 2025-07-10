using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Common;
using CargoWise.SystemToSystemTrust;
using Enterprise.Registry.Business;
using Newtonsoft.Json;
using WTG.IdentitySecurity;
using WTG.OpenIDConnect.Token;

namespace Enterprise.MasterFiles.Business
{
	public static class IdpApiClient
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Bearer string")]
		const string Bearer = "Bearer";

		public static IEnumerable<CreatedUserInfo> CreateUsersBulk(IEnumerable<MigratedUser> users)
		{
			Uri uri;
			try
			{
				uri = IdpEndpointUriProvider.GetBulkCreateUsersUri();
			}
			catch (IdpConfigException ex)
			{
				throw new IdpCreateUsersException(ex.Message);
			}

			var json = JsonConvert.SerializeObject(users);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			using var httpClient = GetHttpClientWithBaseAddress(uri);
			try
			{
				var response = httpClient
					.PostAsync(uri, content)
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

				if (response.IsSuccessStatusCode)
				{
					var responseContent = response.Content.ReadAsStringAsync().Result;
					return JsonConvert.DeserializeObject<List<CreatedUserInfo>>(responseContent);
				}

				throw new IdpCreateUsersException(Res.GetString("4F907EB6-4CF7-4EB4-B9CC-96060620CB25", "Unable to create users. Response: {0}, {1}", response.StatusCode, response.Content.ReadAsStringAsync().Result));
			}
			catch (Exception ex) when (!ex.IsCriticalException() && ex is not IdpCreateUsersException)
			{
				throw new IdpCreateUsersException(inner: ex);
			}
		}

		public static string GetVerifiedUserId(string clientId, string loginName)
		{
			var uri = new Uri($"{IdpEndpointUriProvider.GetIdpUserSyncBaseUri()}/{clientId}/{loginName}");

			using var httpClient = GetHttpClientWithBaseAddress(uri);
			var response = httpClient
				.GetAsync(uri)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult()
				.EnsureSuccessStatusCode();

			var responseContent = response.Content.ReadAsStringAsync().Result;
			return JsonConvert.DeserializeObject<string>(responseContent);
		}

		static HttpClient GetHttpClientWithBaseAddress(Uri uri)
		{
			const string mediaType = "application/json";

			var httpClient = new HttpClient();
			httpClient.BaseAddress = uri;
			httpClient.Timeout = GetRequestTimeoutInSeconds();
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Bearer, GetClientAccessToken());
			return httpClient;
		}

		static string GetClientAccessToken()
		{
			var systemToSystemTrustInfo = SystemToSystemTrustRegistryItemValue;

#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
			if (systemToSystemTrustInfo == null || string.IsNullOrEmpty(systemToSystemTrustInfo.PrivateKey) || string.IsNullOrEmpty(systemToSystemTrustInfo.ClientId) || string.IsNullOrEmpty(systemToSystemTrustInfo.TenantId) || systemToSystemTrustInfo.CertificateBytes.Length == 0)
			{
				throw new UserManagementException(Res.GetString("93329EBF-FBD6-48B7-9B10-4EB48923275E", "Cannot get access token"));
			}
			var accessToken = GetAccessToken(systemToSystemTrustInfo.PrivateKey, systemToSystemTrustInfo.ClientId, systemToSystemTrustInfo.TenantId, systemToSystemTrustInfo.CertificateBytes);
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented

			return accessToken;
		}

		static string GetAccessToken(string privateKeyPem, string clientId, string tenantId, byte[] certificate)
		{
			var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
			var privateKey = RSAKeyProvider.ImportPrivateKey(privateKeyPem);
			var cert = new X509Certificate2(certificate);
			var ediClientID = SystemDataRegistry.Instance.IdentityProviderClientID.Value;
			var accessToken = OAuthClientAssertion.GetClientAccessTokenAsync(url, privateKey, cert, clientId, ediClientID).ConfigureAwait(false).GetAwaiter().GetResult();
			return accessToken;
		}

		static TimeSpan GetRequestTimeoutInSeconds()
		{
			TimeSpan defaultTimeOutInSeconds = TimeSpan.FromSeconds(5);

			var res = SystemDataRegistry.Instance.IdentityProviderTimeoutInSeconds.Value;

			if (res <= 0 || res == int.MaxValue)
			{
				return defaultTimeOutInSeconds;
			}

			return TimeSpan.FromSeconds(res);
		}

		static ISystemToSystemTrustInfo SystemToSystemTrustRegistryItemValue => SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
	}
}
