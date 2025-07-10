using WTG.OpenIDConnect.Token;

namespace Enterprise.MasterFiles.Business
{
	class AuthorizationTokenProvider : IAuthorizationTokenProvider
	{
		public string GetToken(AuthenticationCertificateInfo certificateInfo)
		{
			var url = $"https://login.microsoftonline.com/{certificateInfo.TenantId}/oauth2/v2.0/token";
			return OAuthClientAssertion.GetClientAccessTokenAsync(url, certificateInfo.PrivateKey, certificateInfo.Certificate, certificateInfo.ClientId, certificateInfo.ServerClientId).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}
