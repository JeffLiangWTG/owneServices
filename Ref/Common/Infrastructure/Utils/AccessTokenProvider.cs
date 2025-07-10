using System;
using System.Globalization;
using System.Threading;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class AccessTokenProvider : IAccessTokenProvider, IDisposable
	{
		public AccessTokenProvider(string tenantId, string clientId, string serviceId, string privateKeyFileName, string certificateFileName, int refreshAdvanceInMinutes = 15) : this(tenantId, clientId, serviceId, refreshAdvanceInMinutes)
		{
			certInfoManager = new CertificateInfoManager(privateKeyFileName, certificateFileName);
			oAuthClient = new OAuthClientWrapper();
		}

#if DEBUG
		public AccessTokenProvider(string tenantId, string clientId, string serviceId, ICertificateInfoManager certificateInfoManager, IOAuthClientWrapper oAuthClientWrapper, int refreshAdvanceInMinutes = 15) : this(tenantId, clientId, serviceId, refreshAdvanceInMinutes)
		{
			certInfoManager = certificateInfoManager;
			oAuthClient = oAuthClientWrapper;
		}
#endif

		AccessTokenProvider(string tenantId, string clientId, string serviceId, int refreshAdvanceInMinutes)
		{
			tenantIdentifier = tenantId;
			clientIdentifier = clientId;
			serviceIdentifier = serviceId;
			this.refreshAdvanceInMinutes = refreshAdvanceInMinutes;
		}

		public string GetAccessToken()
		{
			if (IsTokenValid())
			{
				return accessToken;
			}

			var endpoint = $"https://login.microsoftonline.com/{tenantIdentifier}/oauth2/v2.0/token";
			var cert = certInfoManager.GetCertificate();
			var privateKey = certInfoManager.GetPrivateKey();
			if (cert == null || privateKey == null)
			{
#if DEBUG
				accessToken = AccessTokenGenerator.GenerateS2SAccessToken(MergerClientIdForS2S, DateTime.MaxValue);
				tokenExpirationTime = DateTime.MaxValue;
				return accessToken;
#else
				return string.Empty;
#endif
			}

			var isNearToExpire = certInfoManager.IsCertNearToExpire();
			if (isNearToExpire)
			{
				var renewAccessToken = oAuthClient.GetClientAccessTokenAsync(endpoint, privateKey, cert, clientIdentifier, RolloverCertificateServiceId).Result;
				RenewalCertificate(renewAccessToken);
			}

			accessToken = oAuthClient.GetClientAccessTokenAsync(endpoint, privateKey, cert, clientIdentifier, serviceIdentifier).Result;
			var tokenReader = new JwtTokenReader(accessToken);
			var expirationValue = tokenReader.GetClaimValue(JwtRegisteredClaimNames.Exp);
			tokenExpirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expirationValue, CultureInfo.InvariantCulture)).UtcDateTime;

			return accessToken;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		void RenewalCertificate(string renewAccessToken)
		{
			new Thread(() =>
			{
				try
				{
					certInfoManager.RenewalCertificateAsync(clientIdentifier, renewAccessToken).ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Renew Certificate failed due to {ex.Message}");
				}
			}).Start();
		}

		bool IsTokenValid()
		{
			if (string.IsNullOrEmpty(accessToken))
			{
				return false;
			}

			return DateTime.UtcNow.AddMinutes(refreshAdvanceInMinutes) < tokenExpirationTime;
		}

		readonly string tenantIdentifier;
		readonly string clientIdentifier;
		readonly string serviceIdentifier;
		readonly int refreshAdvanceInMinutes;

		readonly ICertificateInfoManager certInfoManager;
		readonly IOAuthClientWrapper oAuthClient;

		string accessToken;
		DateTime tokenExpirationTime;

		const string MergerClientIdForS2S = "12cb4f59-a4dd-4058-9c67-0e614754b796";
		const string RolloverCertificateServiceId = "9A6EBFEF-9638-4FDD-95BB-5394926C0422";

		public void Dispose()
		{
			certInfoManager?.Dispose();
		}
	}
}
