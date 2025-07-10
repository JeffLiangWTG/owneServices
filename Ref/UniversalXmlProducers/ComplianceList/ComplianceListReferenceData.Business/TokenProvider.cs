using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WTG.IdentitySecurity;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Business
{
	public class TokenProvider : ITokenProvider, IDisposable
	{
		readonly HttpClient _client = new();
		public async Task<string> GetAuthorizationTokenAsync(string tenantId, string clientId, string serviceId, string privateKeyFileName, string certificateFileName)
		{
			var tokenEndpoint = string.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/oauth2/v2.0/token", tenantId);
			using (var certificate = new X509Certificate2(certificateFileName))
			{
				var privateKey = RSAKeyProvider.ImportPrivateKey(File.ReadAllText(privateKeyFileName));
				var accessToken = await OAuthClientAssertion.GetClientAccessTokenAsync(tokenEndpoint, privateKey, certificate, clientId, serviceId, CreateClient).ConfigureAwait(false);
				return accessToken;
			}
		}

		public HttpClient CreateClient()
		{
			return _client;
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}